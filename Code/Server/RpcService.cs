using Microsoft.OpenApi.Models;
using Proto;
using Protocol;
using Server.Helper;
using Server.Repo;
using Server.Service;
using ServerCore;
using ServerCore.Serializer;
using WebStudyServer;
using ServerCore.Extension;
using WebStudyServer.Helper;
namespace Server
{
    public class RpcService
    {
        public RpcService(RpcMethodRegistry registry, RpcContext rpcCtx, ResponseCacheService responseCache,
            UserLockService userLockSvc, GlobalDbRepo dbRepo, ILogger<RpcService> logger)
        {
            _registry = registry;
            _rpcCtx = rpcCtx;
            _responseCache = responseCache;
            _userLockSvc = userLockSvc;
            _dbRepo = dbRepo;
            _logger = logger;
        }

        public async Task OnHttpBodyRequestAsync(HttpContext httpCtx, string methodName)
        {
            // Seq 재전송이면 재실행 없이 캐시된 응답을 그대로 반환한다.
            if (_responseCache.TryGet(_rpcCtx, out var cachedBody))
            {
                var cachedContentType = ResWriteHelper.GetOutputContentType(httpCtx);
                await ResWriteHelper.WriteBytesAsync(httpCtx, cachedContentType, cachedBody);
                return;
            }

            // 로그
            var httpMethod = httpCtx.Request.Method;
            var httpPath = httpCtx.Request.Path.ToString();

            var httpReqContentType = CustomInputFormatter.GetContentTypeByHeader(httpCtx);
            if (!_contentTypeToSerializerDict.TryGetValue(httpReqContentType, out var rpcReqSerializer))
            {
                httpCtx.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return;
            }

            if (!_registry.NameToMethodDict.TryGetValue(methodName, out var rpcMethod))
            {
                throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NOT_FOUND_METHOD", new { MethodName = methodName });
            }

            var httpReqStream = httpCtx.Request.Body;
            var rpcReqObj = await rpcReqSerializer.DeserializeAsync(rpcMethod.Req, httpReqStream);
            if (rpcReqObj == null)
            {
                httpCtx.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            // TODO: Logger 수정하고 적용 (Body가 메시지로 나오면 안되고 arg에만 들어가게)
            //var args = new Dictionary<string, object>()
            //{
            //    { "Method", httpMethod },
            //    { "Path", httpPath },
            //    { "Body", rpcReqObj},
            //};

            _logger.Info("Req Method({Method}) Path({Path}) Body({Body})", httpMethod, httpPath, rpcReqObj);

            // 예외는 여기서 잡지 않고 전역 UseExceptionHandler(ErrorHandler)로 위임한다.
            var rpcResObj = await HandleMethodAsync(httpCtx, rpcMethod, rpcReqObj);

            _logger.Info("Res Method({Method}) Path({Path}) Body({Body})", httpMethod, httpPath, rpcResObj);
        }

        private async Task<object> HandleMethodAsync(HttpContext httpCtx, IRpcMethod rpcMethod, object rpcReqObj)
        {
            object rpcResObj = null;
            var contentType = ResWriteHelper.GetOutputContentType(httpCtx);
            byte[] resBody = null;
            try
            {
                await _userLockSvc.RunAtomicAsync(_rpcCtx.AccountId, async () =>
                {
                    rpcResObj = await rpcMethod.RunAsync(_rpcCtx, httpCtx, _dbRepo, rpcReqObj);
                });

                resBody = _contentTypeToSerializerDict[contentType].Serialize(rpcResObj);
                _responseCache.Set(_rpcCtx, resBody);

                _dbRepo.Commit();
            }
            catch (Exception)
            {
                _dbRepo.Rollback();
                throw; // 오류 발생 시 ErrorHandler에서 처리
            }

            await ResWriteHelper.WriteBytesAsync(httpCtx, contentType, resBody);
            return rpcResObj;
        }

        private readonly RpcMethodRegistry _registry;
        private readonly RpcContext _rpcCtx;
        private readonly ResponseCacheService _responseCache;
        private readonly UserLockService _userLockSvc;
        private readonly GlobalDbRepo _dbRepo;
        private readonly ILogger<RpcService> _logger;

        private readonly Dictionary<string, IDataSerializer> _contentTypeToSerializerDict = new()
        {
            {MsgProtocol.JsonContentType, new JsonDataSerializer()},
            {MsgProtocol.ProtoBufContentType, new ProtoBufDataSerializer()},
        };
    }

    public static class RpcServiceExtension
    {
        // 등록된 모든 메소드를 pattern에 매핑
        public static void MapAllPostRpc(this WebApplication app, string pattern)
        {
            var registry = app.Services.GetRequiredService<RpcMethodRegistry>();

            foreach (var keyPair in registry.NameToMethodDict)
            {
                var methodName = keyPair.Key;
                var rpcMethod = keyPair.Value;
                app.MapPost($"{pattern}/{methodName}", async (RpcService rpcSvc, HttpContext httpCtx) =>
                {
                    await rpcSvc.OnHttpBodyRequestAsync(httpCtx, methodName);
                }).WithOpenApi((op) => new OpenApiOperation
                {
                    RequestBody = OpenApiHelper.CreateRequestBody(rpcMethod.Req),
                    Responses = OpenApiHelper.CreateResponse(rpcMethod.Res)
                });
            }
        }
    }
}

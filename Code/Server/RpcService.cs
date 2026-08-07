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
        public RpcService(List<IRpcMethod> methodList, ILogger<RpcService> logger)
        {
            _logger = logger;

            foreach (var method in methodList)
            {
                _nameToMethodDict.Add(method.Name, method);
            }
        }

        public async Task OnHttpBodyRequestAsync(HttpContext httpCtx, string methodName)
        {
            var rpcCtx = httpCtx.RequestServices.GetRequiredService<RpcContext>();

            // Seq 재전송이면 재실행 없이 캐시된 응답을 그대로 반환한다.
            var responseCache = httpCtx.RequestServices.GetRequiredService<ResponseCacheService>();
            if (responseCache.TryGet(rpcCtx, out var cachedBody))
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

            if (!NameToMethodDict.TryGetValue(methodName, out var rpcMethod))
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
            var rpcResObj = await HandleMethodAsync(rpcCtx, httpCtx, rpcMethod, rpcReqObj, responseCache);

            _logger.Info("Res Method({Method}) Path({Path}) Body({Body})", httpMethod, httpPath, rpcResObj);
        }

        private async Task<object> HandleMethodAsync(RpcContext rpcCtx, HttpContext httpCtx, IRpcMethod rpcMethod, object rpcReqObj, ResponseCacheService responseCache)
        {
            var userLockSvc = httpCtx.RequestServices.GetRequiredService<UserLockService>();
            var dbRepo = httpCtx.RequestServices.GetRequiredService<GlobalDbRepo>();
            object rpcResObj = null;
            var contentType = ResWriteHelper.GetOutputContentType(httpCtx);
            byte[] resBody = null;
            try
            {
                await userLockSvc.RunAtomicAsync(rpcCtx.AccountId, async () =>
                {
                    rpcResObj = await rpcMethod.RunAsync(rpcCtx, httpCtx, dbRepo, rpcReqObj);
                });

                resBody = _contentTypeToSerializerDict[contentType].Serialize(rpcResObj);
                responseCache.Set(rpcCtx, resBody);

                dbRepo.Commit();
            }
            catch (Exception)
            {
                dbRepo.Rollback();
                throw; // 오류 발생 시 ErrorHandler에서 처리
            }

            await ResWriteHelper.WriteBytesAsync(httpCtx, contentType, resBody);
            return rpcResObj;
        }

        public IReadOnlyDictionary<string, IRpcMethod> NameToMethodDict => _nameToMethodDict;
        private readonly Dictionary<string, IRpcMethod> _nameToMethodDict = [];

        private readonly ILogger<RpcService> _logger;

        private readonly Dictionary<string, IDataSerializer> _contentTypeToSerializerDict = new()
        {
            {MsgProtocol.JsonContentType, new JsonDataSerializer()},
            {MsgProtocol.ProtoBufContentType, new ProtoBufDataSerializer()},
        };
    }

    public static class RpcServiceExtension
    {
        // RpcService에 등록된 모든 메소드를 pattern에 매핑
        public static void MapAllPostRpc(this WebApplication app, string pattern)
        {
            var rpcSvc = app.Services.GetRequiredService<RpcService>();

            foreach (var keyPair in rpcSvc.NameToMethodDict)
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

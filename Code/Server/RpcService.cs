using Microsoft.OpenApi.Models;
using Proto;
using Protocol;
using Server.Helper;
using Server.Service;
using ServerCore;
using ServerCore.Serializer;
using WebStudyServer;
using ServerCore.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Data;
namespace Server
{
    public class RpcService
    {
        public RpcService(RpcMethodRegistry registry, RpcContext rpcCtx, ResponseCacheService responseCache,
            UserLockService userLockSvc, GameDb db, ILogger<RpcService> logger)
        {
            _registry = registry;
            _rpcCtx = rpcCtx;
            _responseCache = responseCache;
            _userLockSvc = userLockSvc;
            _db = db;
            _logger = logger;
        }

        public async Task OnHttpBodyRequestAsync(HttpContext httpCtx, string methodName)
        {
            // 점검모드면 여기서 차단 (캐시된 응답 재전송 포함, RPC 전체를 막아야 함)
            CancelReqException.ThrowCancelRequestException(httpCtx);
            await _rpcCtx.InitAsync(httpCtx);

            if (!_registry.NameToMethodDict.TryGetValue(methodName, out var rpcMethod))
            {
                throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NOT_FOUND_METHOD", new { MethodName = methodName });
            }

            // 요청은 캐시 히트 여부와 무관하게 항상 파싱함.
            var rpcReqObj = await ParseRequestAsync(httpCtx, rpcMethod);
            if (rpcReqObj == null)
            {
                return; // 415/400 - 응답은 ParseRequestAsync에서 이미 작성됨
            }

            _logger.Info("Req Method({Method}) Path({Path}) Body({Body})", httpCtx.Request.Method, httpCtx.Request.Path.ToString(), rpcReqObj);

            // Seq 재전송이면 재실행 없이 캐시된 응답 객체를 그대로 쓴다.
            var (cacheHit, cachedObj) = await _responseCache.TryGetAsync(_rpcCtx, rpcMethod.Res);
            var resObj = cacheHit ? cachedObj : await HandleMethodAsync(httpCtx, rpcMethod, rpcReqObj);

            _logger.Info("Res Method({Method}) Path({Path}) CacheHit({CacheHit}) Body({Body})",httpCtx.Request.Method, httpCtx.Request.Path.ToString(), cacheHit, resObj);

            var contentType = ResWriteHelper.GetOutputContentType(httpCtx);
            var resBody = _registry.ContentTypeToSerializerDict[contentType].Serialize(resObj);
            await ResWriteHelper.WriteBytesAsync(httpCtx, contentType, resBody);
        }

        // ContentType 협상 → 역직렬화, 실패 시 415/400 응답 후 null 반환.
        private async Task<object> ParseRequestAsync(HttpContext httpCtx, IRpcMethod rpcMethod)
        {
            var httpReqContentType = CustomInputFormatter.GetContentTypeByHeader(httpCtx);
            if (!_registry.ContentTypeToSerializerDict.TryGetValue(httpReqContentType, out var rpcReqSerializer))
            {
                httpCtx.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return null;
            }

            var rpcReqObj = await rpcReqSerializer.DeserializeAsync(rpcMethod.Req, httpCtx.Request.Body);
            if (rpcReqObj == null)
            {
                httpCtx.Response.StatusCode = StatusCodes.Status400BadRequest;
                return null;
            }

            return rpcReqObj;
        }

        private async Task<object> HandleMethodAsync(HttpContext httpCtx, IRpcMethod rpcMethod, object rpcReqObj)
        {
            object rpcResObj = null;

            // 쓰기·커밋·롤백이 모두 유저 락 안에 있어야 한다. 커밋이 밖으로 나가면
            // 같은 계정의 동시 요청이 서로의 결과를 덮어쓸 수 있다(lost update).
            await _userLockSvc.RunAtomicAsync(_rpcCtx.AccountId, async () =>
            {
                try
                {
                    rpcResObj = await rpcMethod.RunAsync(_rpcCtx, httpCtx, rpcReqObj);

                    // 응답 캐시 쓰기는 커밋보다 앞이어야 한다 — 캐시 세션의 pending에 쌓였다가
                    // CommitAsync에서 DB 커밋 후 함께 반영되고, 롤백 시 같이 버려진다.
                    await _responseCache.SetAsync(_rpcCtx, rpcResObj);

                    await _db.CommitAsync();
                }
                catch (Exception)
                {
                    await _db.RollbackAsync();
                    throw; // 오류 발생 시 ErrorHandler에서 처리
                }
            });

            return rpcResObj;
        }

        private readonly RpcMethodRegistry _registry;
        private readonly RpcContext _rpcCtx;
        private readonly ResponseCacheService _responseCache;
        private readonly UserLockService _userLockSvc;
        private readonly GameDb _db;
        private readonly ILogger<RpcService> _logger;
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

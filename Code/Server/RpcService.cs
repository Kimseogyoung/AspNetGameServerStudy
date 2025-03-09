using Microsoft.OpenApi.Models;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using Server.Serializer;
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

        public async Task OnHttpBodyRequestAsync(HttpContext httpCtx, string pattern)
        {
            var rpcCtx = httpCtx.RequestServices.GetRequiredService<RpcContext>();

            // 로그 
            var httpMethod = httpCtx.Request.Method;
            var httpPath = httpCtx.Request.Path.ToString();
            object rpcResObj = null;
            try
            {
                var httpReqContentType = CustomInputFormatter.GetContentTypeByHeader(httpCtx);
                if (!_contentTypeToSerializerDict.TryGetValue(httpReqContentType, out var rpcReqSerializer))
                {
                    httpCtx.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return;
                }

                var methodName = GetMethodNameFromPath(httpCtx, pattern);
                if (!_nameToMethodDict.TryGetValue(methodName, out var rpcMethod))
                {
                    throw new GameException("NOT_FOUND_METHOD", new { MethodName = methodName });
                }

                var httpReqStream = httpCtx.Request.Body;
                var rpcReqObj = await rpcReqSerializer.DeserializeAsync(rpcMethod.Req, httpReqStream);
                if (rpcReqObj == null)
                {
                    httpCtx.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                // TODO: Response Cache
                //

                // TODO: Logger 수정하고 적용 (Body가 메시지로 나오면 안되고 arg에만 들어가게)
                //var args = new Dictionary<string, object>()
                //{
                //    { "Method", httpMethod },
                //    { "Path", httpPath },
                //    { "Body", rpcReqObj},
                //};

                _logger.Info("Req Method({Method}) Path({Path}) Body({Body})", httpMethod, httpPath, rpcReqObj);
                rpcResObj = await HandleMethod(rpcCtx, httpCtx, rpcMethod, rpcReqObj);
            }
            catch (Exception exc)
            {
                var errorSvc = httpCtx.RequestServices.GetRequiredService<ErrorHandler>();
                rpcResObj = await errorSvc.HandleWithExceptionAsnyc(httpCtx, exc);
            }
            finally
            {
                _logger.Info("Res Method({Method}) Path({Path}) Body({Body})", httpMethod, httpPath, rpcResObj);
            }
        }

        private async Task<object> HandleMethod(RpcContext rpcCtx, HttpContext httpCtx, IRpcMethod rpcMethod, object rpcReqObj)
        {
            var userLockSvc = httpCtx.RequestServices.GetRequiredService<UserLockService>();
            var dbRepo = httpCtx.RequestServices.GetRequiredService<DbRepo>();
            object rpcResObj = null;
            try
            {
                await userLockSvc.RunAtomicAsync(rpcCtx.AccountId, dbRepo, async () =>
                {
                    rpcResObj = await rpcMethod.RunAsync(rpcCtx, httpCtx, rpcReqObj);
                    await ResWriteHelper.WriteResponseBodyAsync(httpCtx, rpcResObj, rpcMethod.Res);
                });

                dbRepo.Commit();
            }
            catch (Exception)
            {
                dbRepo.Rollback();
                throw; // 오류 발생 시 ErrorHandler에서 처리
            }

            return rpcResObj;
        }

        private string GetMethodNameFromPath(HttpContext httpCtx, string pattern)
        {
            var path = httpCtx.Request.Path;
            var methodName = path.Value.Replace($"/{pattern}/", "");
            return methodName;
        }

        public Dictionary<string, IRpcMethod> NameToMethodDict => _nameToMethodDict;

        private readonly Dictionary<string, IRpcMethod> _nameToMethodDict = new Dictionary<string, IRpcMethod>();
        private readonly ILogger<RpcService> _logger;

        private Dictionary<string, IDataSerializer> _contentTypeToSerializerDict = new()
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
                app.MapPost($"{pattern}/{methodName}", async (RpcService rpcSvc, HttpContext httpCtx) =>
                {
                    await rpcSvc.OnHttpBodyRequestAsync(httpCtx, pattern);
                }).WithOpenApi((op) => new OpenApiOperation
                {
                  
                    RequestBody = keyPair.Value.CreateOpenApiRequestBody(),
                    //Parameters = keyPair.Value.CreateOpenApiParameters(),
                    Responses = keyPair.Value.CreateOpenApiResponse()
                }); ;
            }
        }
    }
}

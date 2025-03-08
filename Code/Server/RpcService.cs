using AutoMapper;
using Microsoft.OpenApi.Models;
using Protocol;
using Server.Formatter;
using Server.Helper;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using System.Text.Json;
namespace Server
{
    public class RpcMethod<SVC, REQ, RES> : IRpcMethod where SVC : class where RES : IResPacket where REQ : IReqPacket, new ()
    {
        public delegate Task<RES> RunAsyncDelegate(SVC svc, REQ req);
        public delegate RES RunDelegate(SVC svc, REQ req);

        public string Name => _name;
        public Type Req => _req;
        public Type Res => _res;

        public RpcMethod(string name, RunAsyncDelegate runAsync)
        {
            _name = name;
            _runAsync = runAsync;

            _req = typeof(REQ);
            _res = typeof(RES);
        }

        public RpcMethod(string name, RunDelegate run)
        {
            _name = name;
            _run = run;
        
            _req = typeof(REQ);
            _res = typeof(RES);
        }

        public async Task<object> RunAsync(HttpContext httpCtx, object rpcReq)
        {
            var rpcSvc = httpCtx.RequestServices.GetRequiredService<SVC>();
            if (_runAsync == null)
            {
                if (_run == null)
                {
                    throw new NullReferenceException("NOT_INITIALIZED_RPC_METHOD_DELEGATE");
                }
                else
                {
                    var res = await Task.Run(() => _run!(rpcSvc, (REQ)rpcReq));
                    return res;
                }
            }
            else
            {
                var res = await _runAsync(rpcSvc, (REQ)rpcReq);
                return res;
            }
        }

        public List<OpenApiParameter> CreateOpenApiParameters()
        {
            return OpenApiHelper.CreateParameters(typeof(REQ));
        }

        public OpenApiRequestBody CreateOpenApiRequestBody()
        {
            return OpenApiHelper.CreateRequestBody(typeof(REQ));
        }

        public OpenApiResponses CreateOpenApiResponse()
        {
            return OpenApiHelper.CreateResponse(typeof(RES));
        }

        private readonly string _name;
        private readonly Type _req;
        private readonly Type _res;
        private readonly RunAsyncDelegate _runAsync;
        private readonly RunDelegate _run;

    }

    public interface IRpcMethod
    {
        public Type Req { get; }
        public Type Res { get; }
        string Name { get; }
        Task<object> RunAsync(HttpContext httpCt, object rpcReq);
        List<OpenApiParameter> CreateOpenApiParameters();
        OpenApiRequestBody CreateOpenApiRequestBody();
        OpenApiResponses CreateOpenApiResponse();
    }

    public interface IDataSerializer
    {
        byte[] Serialize<T>(T inObj);

        Task SerializeAsync<T>(Stream inStream, T inObj);

        Task<T> DeserializeAsync<T>(Stream inStream);

        Task<object> DeserializeAsync(Type type, Stream inStream);

        string ContentType { get; }
    }

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

            // TODO: Response Cache
            //

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

            var userLockSvc = httpCtx.RequestServices.GetRequiredService<UserLockService>();
            var dbRepo = httpCtx.RequestServices.GetRequiredService<DbRepo>();
            await userLockSvc.RunAtomicAsync(rpcCtx.AccountId, dbRepo.Auth, async () =>
            {
                
                try
                {
                    var rpcResObj = await rpcMethod.RunAsync(httpCtx, rpcReqObj);
                    await ResWriteHelper.WriteResponseBodyAsync(httpCtx, rpcResObj, rpcMethod.Res);
                }
                catch (GameException)
                {
                    // TODO: 동작 보고 처리
                    throw;
                }
                catch (Exception)
                {
                    // TODO: 동작 보고 처리
                    throw;
                }
            });
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
        public static void MapAllPostRpc(this WebApplication app, string pattern)
        {
            var rpcSvc = app.Services.GetRequiredService<RpcService>();

            foreach (var keyPair in rpcSvc.NameToMethodDict)
            {
                var methodName = keyPair.Key;
                app.MapPost($"{pattern}/{methodName}", async (RpcService rpcSvc, HttpContext httpCtx) =>
                {
                    await rpcSvc.OnHttpBodyRequestAsync(httpCtx, pattern);
                }).WithOpenApi((op) => new Microsoft.OpenApi.Models.OpenApiOperation
                {
                  
                    RequestBody = keyPair.Value.CreateOpenApiRequestBody(),
                    //Parameters = keyPair.Value.CreateOpenApiParameters(),
                    Responses = keyPair.Value.CreateOpenApiResponse()
                }); ;
            }
        }
    }
}

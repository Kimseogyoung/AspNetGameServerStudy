using Microsoft.OpenApi.Models;
using Protocol;
using Server.Helper;
using WebStudyServer.Helper;
using WebStudyServer;
using Server.Repo;

namespace Server
{
    public class RpcMethod<SVC, REQ, RES> : IRpcMethod where SVC : class where RES : IResPacket where REQ : IReqPacket, new()
    {
        public delegate Task<RES> RunAsyncDelegate(SVC svc, REQ req);
        public delegate RES RunDelegate(SVC svc, REQ req);

        public string Name => _name;
        public Type Req => _req;
        public Type Res => _res;


        public RpcMethod()
        {
            _req = typeof(REQ);
            _res = typeof(RES);
        }

        public RpcMethod(string name, RunAsyncDelegate runAsync, ERpcMethodType type = ERpcMethodType.NONE)
        {
            _name = name;
            _runAsync = runAsync;
            _type = type;
            _req = typeof(REQ);
            _res = typeof(RES);
        }

        public RpcMethod(string name, RunDelegate run, ERpcMethodType type = ERpcMethodType.NONE)
        {
            _name = name;
            _run = run;
            _type = type;
            _req = typeof(REQ);
            _res = typeof(RES);
        }

        public async Task<object> RunAsync(RpcContext rpcCtx, HttpContext httpCtx, object rpcReq)
        {
            // 여기서 처리해야하는지는 의문임.
            switch (_type)
            {
                case ERpcMethodType.NONE:
                    break;
                case ERpcMethodType.AUTHORIZED:
                    {
                        ReqHelper.ValidContext(rpcCtx.SessionLoadState == RpcContext.ESessionLoadState.LOADED, "FAILED_SESSION_LOAD", () => new { SessionKey = rpcCtx.SessionKey, SessionLoadState = rpcCtx.SessionLoadState });
                        ReqHelper.ValidContext(rpcCtx.AccountId != 0, "NOT_FOUND_ACCOUNT_IN_RPC_METHOD_RUN", () => new { SessionKey = rpcCtx.SessionKey });
                        var dbRepo = httpCtx.RequestServices.GetRequiredService<DbRepo>();
                        dbRepo.BeginOwnUserRepo();
                        break;
                    }
                case ERpcMethodType.AUTHORIZED_PLAYER:
                    {
                        ReqHelper.ValidContext(rpcCtx.SessionLoadState == RpcContext.ESessionLoadState.LOADED, "FAILED_SESSION_LOAD", () => new { SessionKey = rpcCtx.SessionKey, SessionLoadState = rpcCtx.SessionLoadState });
                        ReqHelper.ValidContext(rpcCtx.AccountId != 0, "NOT_FOUND_ACCOUNT_IN_RPC_METHOD_RUN", () => new { SessionKey = rpcCtx.SessionKey });
                        ReqHelper.ValidContext(rpcCtx.PlayerId != 0, "NOT_FOUND_PLAYER_IN_RPC_METHOD_RUN", () => new { SessionKey = rpcCtx.SessionKey, AccountId = rpcCtx.AccountId });
                        var dbRepo = httpCtx.RequestServices.GetRequiredService<DbRepo>();
                        dbRepo.BeginOwnUserRepo();
                        break;
                    }
                case ERpcMethodType.OPS:
                    break;
                default:
                    throw new Exception($"NO_HANDLING_RPC_METHOD_TYPE:{_type}");
            }

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


        private readonly ERpcMethodType _type;
        private readonly string _name;
        private readonly Type _req;
        private readonly Type _res;
        private readonly RunAsyncDelegate _runAsync;
        private readonly RunDelegate _run;

    }
}

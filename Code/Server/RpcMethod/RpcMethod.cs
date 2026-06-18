using Microsoft.OpenApi.Models;
using Proto;
using Protocol;
using Server.Helper;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Helper;

namespace Server
{
    public class RpcMethod<TSvc, TReq, TRes> : IRpcMethod where TSvc : class where TRes : IResponsePacket where TReq : IRequestPacket, new()
    {
        public delegate Task<TRes> RunAsyncDelegate(TSvc svc, TReq req);
        public delegate TRes RunDelegate(TSvc svc, TReq req);

        public string Name { get; }
        public Type Req { get; }
        public Type Res { get; }


        public RpcMethod()
        {
            Req = typeof(TReq);
            Res = typeof(TRes);
        }

        public RpcMethod(string name, RunAsyncDelegate runAsync, ERpcMethodType type = ERpcMethodType.NONE)
        {
            Name = name;
            _runAsync = runAsync;
            _type = type;
            Req = typeof(TReq);
            Res = typeof(TRes);
        }

        public RpcMethod(string name, RunDelegate run, ERpcMethodType type = ERpcMethodType.NONE)
        {
            Name = name;
            _run = run;
            _type = type;
            Req = typeof(TReq);
            Res = typeof(TRes);
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
                        ValidateSession(rpcCtx);
                        ReqHelper.Valid(rpcCtx.AccountId != 0, EErrorCode.CONTEXT_ACCOUNT, () => new { rpcCtx.SessionKey });
                        var dbRepo = httpCtx.RequestServices.GetRequiredService<GlobalDbRepo>();
                        dbRepo.BeginOwnUserRepo();
                        break;
                    }
                case ERpcMethodType.AUTHORIZED_PLAYER:
                    {
                        ValidateSession(rpcCtx);
                        ReqHelper.Valid(rpcCtx.AccountId != 0, EErrorCode.CONTEXT_ACCOUNT, () => new { rpcCtx.SessionKey });
                        ReqHelper.Valid(rpcCtx.PlayerId != 0, EErrorCode.CONTEXT_PLAYER, () => new { rpcCtx.SessionKey, rpcCtx.AccountId });
                        var dbRepo = httpCtx.RequestServices.GetRequiredService<GlobalDbRepo>();
                        dbRepo.BeginOwnUserRepo();
                        break;
                    }
                case ERpcMethodType.OPS:
                    break;
                default:
                    throw new Exception($"NO_HANDLING_RPC_METHOD_TYPE:{_type}");
            }

            var rpcSvc = httpCtx.RequestServices.GetRequiredService<TSvc>();
            if (_runAsync == null)
            {
                if (_run == null)
                {
                    throw new NullReferenceException("NOT_INITIALIZED_RPC_METHOD_DELEGATE");
                }
                else
                {
                    var res = await Task.Run(() => _run!(rpcSvc, (TReq)rpcReq));
                    res.Info = new ResponseInfoPacket { ResultCode = (int)EErrorCode.OK };
                    return res;
                }
            }
            else
            {
                var res = await _runAsync(rpcSvc, (TReq)rpcReq);
                res.Info = new ResponseInfoPacket { ResultCode = (int)EErrorCode.OK };
                return res;
            }
        }

        public List<OpenApiParameter> CreateOpenApiParameters()
        {
            return OpenApiHelper.CreateParameters(typeof(TReq));
        }

        public OpenApiRequestBody CreateOpenApiRequestBody()
        {
            return OpenApiHelper.CreateRequestBody(typeof(TReq));
        }

        public OpenApiResponses CreateOpenApiResponse()
        {
            return OpenApiHelper.CreateResponse(typeof(TRes));
        }


        private static void ValidateSession(RpcContext rpcCtx)
        {
            switch (rpcCtx.SessionLoadState)
            {
                case RpcContext.ESessionLoadState.LOADED:
                    return;
                case RpcContext.ESessionLoadState.NOT_FOUND:
                    throw new GameException(EErrorCode.SESSION_NOT_FOUND, "SESSION_NOT_FOUND",
                        new { rpcCtx.SessionKey });
                case RpcContext.ESessionLoadState.EXPIRED:
                    throw new GameException(EErrorCode.SESSION_EXPIRED, "SESSION_EXPIRED",
                        new { rpcCtx.SessionKey });
                default:
                    throw new GameException(EErrorCode.CONTEXT, "FAILED_SESSION_LOAD",
                        new { rpcCtx.SessionKey, rpcCtx.SessionLoadState });
            }
        }

        private readonly ERpcMethodType _type;
        private readonly RunAsyncDelegate _runAsync;
        private readonly RunDelegate _run;

    }
}

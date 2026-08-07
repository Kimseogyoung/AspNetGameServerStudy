using Proto;
using Protocol;
using Server.Repo;
using WebStudyServer;

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

        public RpcMethod(string name, RunAsyncDelegate runAsync, IRpcAuthPolicy authPolicy = null)
        {
            Name = name;
            _runAsync = runAsync;
            _authPolicy = authPolicy;
            Req = typeof(TReq);
            Res = typeof(TRes);
        }

        public RpcMethod(string name, RunDelegate run, IRpcAuthPolicy authPolicy = null)
        {
            Name = name;
            _run = run;
            _authPolicy = authPolicy;
            Req = typeof(TReq);
            Res = typeof(TRes);
        }

        public async Task<object> RunAsync(RpcContext rpcCtx, HttpContext httpCtx, GlobalDbRepo dbRepo, object rpcReq)
        {
            _authPolicy?.Validate(rpcCtx);
            if (_authPolicy?.RequiresUserRepo == true)
            {
                dbRepo.BeginOwnUserRepo();
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

        private readonly IRpcAuthPolicy _authPolicy;
        private readonly RunAsyncDelegate _runAsync;
        private readonly RunDelegate _run;

    }
}

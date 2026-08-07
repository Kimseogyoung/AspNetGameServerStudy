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

        public async Task<object> RunAsync(RpcContext rpcCtx, HttpContext httpCtx, object rpcReq)
        {
            _authPolicy?.Validate(rpcCtx);
            if (_authPolicy?.RequiresUserRepo == true)
            {
                httpCtx.RequestServices.GetRequiredService<GlobalDbRepo>().BeginOwnUserRepo();
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


        private readonly IRpcAuthPolicy _authPolicy;
        private readonly RunAsyncDelegate _runAsync;
        private readonly RunDelegate _run;

    }
}

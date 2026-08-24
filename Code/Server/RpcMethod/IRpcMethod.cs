using WebStudyServer;

namespace Server
{
    public interface IRpcMethod
    {
        public Type Req { get; }
        public Type Res { get; }
        string Name { get; }
        Task<object> RunAsync(RpcContext rpcCtx, HttpContext httpCtx, object rpcReq);
    }
}

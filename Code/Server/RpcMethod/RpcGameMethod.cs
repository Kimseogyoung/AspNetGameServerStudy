using Microsoft.OpenApi.Models;
using Protocol;
using Server.Helper;

namespace Server
{
    public class RpcGameMethod<TSvc, TReq, TRes> : RpcMethod<TSvc, TReq, TRes> where TSvc : class where TRes : IResponsePacket where TReq : IRequestPacket, new()
    {
        public RpcGameMethod(string name, RunAsyncDelegate runAsync, bool includePlayer = true)
            : base(name, runAsync, includePlayer ? PlayerAuthPolicy.Instance : AccountAuthPolicy.Instance) { }
        public RpcGameMethod(string name, RunDelegate run, bool includePlayer = true)
            : base(name, run, includePlayer ? PlayerAuthPolicy.Instance : AccountAuthPolicy.Instance) { }
    }
}

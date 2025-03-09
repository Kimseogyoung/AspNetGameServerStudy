using Microsoft.OpenApi.Models;
using Protocol;
using Server.Helper;

namespace Server
{
    public class RpcGameMethod<SVC, REQ, RES> : RpcMethod<SVC, REQ, RES> where SVC : class where RES : IResPacket where REQ : IReqPacket, new()
    {
        public RpcGameMethod(string name, RunAsyncDelegate runAsync, bool includePlayer = true) 
            : base(name, runAsync, includePlayer ? ERpcMethodType.AUTHORIZED_PLAYER : ERpcMethodType.AUTHORIZED) { }
        public RpcGameMethod(string name, RunDelegate run, bool includePlayer = true)
            : base(name, run, includePlayer ? ERpcMethodType.AUTHORIZED_PLAYER : ERpcMethodType.AUTHORIZED) { }
    }
}

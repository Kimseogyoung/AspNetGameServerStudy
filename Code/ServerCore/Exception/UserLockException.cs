using Proto;

namespace ServerCore
{
    public class UserLockException : Exception, IServerExpectedException
    {
        public ulong AccountId { get; private set; }
        public int Code { get; private set; }
        public string InternalErrMsg { get; private set; }

        public int ErrorCode => Code;
        public object ErrorArgs => new { AccountId };

        public UserLockException(ulong accountId, string message, string internalErrMsg = "", Exception inner = null) : base(message, inner)
        {
            Code = (int)EErrorCode.USER_LOCK;
            AccountId = accountId;
            InternalErrMsg = internalErrMsg;
        }

    }
}

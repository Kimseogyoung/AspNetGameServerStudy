using Proto;

namespace ServerCore
{
    public class UserLockException : Exception
    {
        public ulong AccountId { get; private set; }
        public int Code { get; private set; }
        public string InternalErrMsg { get; private set; }
        public UserLockException(ulong accountId, string message, string internalErrMsg = "") : base(message)
        {
            Code = (int)EErrorCode.USER_LOCK;
            AccountId = accountId;
            InternalErrMsg = internalErrMsg;
        }

    }
}

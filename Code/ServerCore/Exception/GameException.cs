using Proto;

namespace ServerCore
{
    public class GameException : Exception, IServerExpectedException
    {
        public int ErrorCode { get; private set; }

        public object ErrorArgs { get; private set; }


        public GameException(EErrorCode code, string message, dynamic args) : base(message)
        {
            ErrorCode = (int)code;
            ErrorArgs = args;
        }

        public GameException(int code, string message, dynamic args) : base(message)
        {
            ErrorCode = code;
            ErrorArgs = args;
        }

        public GameException(int code, string message) : base(message)
        {
            ErrorCode = code;
        }

        [Obsolete("에러코드를 명시하는 생성자를 사용하세요. 예: GameException(EErrorCode.NO_HANDLING_ERROR, message, args)")]
        public GameException(string message, dynamic args) : base(message)
        {
            ErrorCode = -1;
            ErrorArgs = args;
        }

        [Obsolete("에러코드를 명시하는 생성자를 사용하세요. 예: GameException(EErrorCode.NO_HANDLING_ERROR, message)")]
        public GameException(string message) : base(message)
        {
            ErrorCode = -1;
        }
    }
}

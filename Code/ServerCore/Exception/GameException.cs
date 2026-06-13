using Proto;

namespace ServerCore
{
    public class GameException : Exception
    {
        public int Code { get; private set; }

        public dynamic Args { get; private set; }


        public GameException(EErrorCode code, string message, dynamic args) : base(message)
        {
            Code = (int)code;
            Args = args;
        }

        public GameException(int code, string message, dynamic args) : base(message)
        {
            Code = code;
            Args = args;
        }

        public GameException(int code, string message) : base(message)
        {
            Code = code;
        }

        [Obsolete("에러코드를 명시하는 생성자를 사용하세요. 예: GameException(EErrorCode.NO_HANDLING_ERROR, message, args)")]
        public GameException(string message, dynamic args) : base(message)
        {
            Code = -1;
            Args = args;
        }

        [Obsolete("에러코드를 명시하는 생성자를 사용하세요. 예: GameException(EErrorCode.NO_HANDLING_ERROR, message)")]
        public GameException(string message) : base(message)
        {
            Code = -1;
        }
    }
}

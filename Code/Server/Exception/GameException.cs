using Proto;

namespace WebStudyServer
{
    public class GameException : Exception
    {
        public int Code {  get; private set; }

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

        public GameException(string message, dynamic args) : base(message)
        {
            Code = -1;
            Args = args;
        }

        public GameException(string message) : base(message)
        {
            Code = -1;
        }
    }
}

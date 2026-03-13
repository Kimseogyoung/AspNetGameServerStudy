using Proto;

namespace WebStudyServer
{
    public class CancelReqException : Exception
    {
        public EErrorCode ErrCode { get; private set; } = EErrorCode.CANCELED_OPERATION;
        public string ApiPath { get; private set; }
        public CancelReqException(string apiPath) : base("CANCEL_OPERATION")
        {
            ApiPath = apiPath;
        }

        public static void ThrowCancelRequestException(HttpContext httpContext)
        {
            if (httpContext.RequestAborted.IsCancellationRequested)
            {
                // 클라쪽에서 캔슬된 요청임
                throw new CancelReqException(httpContext.Request.Path);
            }
        }
    }
}

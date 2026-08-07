using Microsoft.AspNetCore.Diagnostics;
using Proto;
using Protocol;
using ServerCore;
using ServerCore.Extension;
using WebStudyServer.Helper;

namespace WebStudyServer
{
    public class ErrorHandler
    {
        public ErrorHandler(ILogger<ErrorHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(HttpContext httpContext)
        {
            var exception = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            if (exception == null)
            {
                _logger.Warnning("EXCEPTION_IS_NULL");
                return;
            }

            var errorCode = (int)EErrorCode.NO_HANDLING_ERROR;
            var errorMsg = exception.Message;
            var errorHash = HashHelper.CalculateMD5Hash(errorMsg)[..6];
            var isExpected = exception is IServerExpectedException;
            object errorArgsObj = null;
            if (exception is IServerExpectedException expectedExc)
            {
                errorCode = expectedExc.ErrorCode;
                errorArgsObj = expectedExc.ErrorArgs;
            }

            var errorArgs = errorArgsObj != null
                ? System.Text.Json.JsonSerializer.Serialize(errorArgsObj)
                : "";

            _logger.Error(exception, "Error:{Code}:{Hash}:{Msg} Args({Args})", errorCode, errorHash, errorMsg, errorArgs);

            // TODO: 에러 리포트
            // sentry

            // 의도된 예외(IServerExpectedException)의 메시지는 개발자가 API용으로 작성한 안전한 문구라
            // 그대로 노출한다. 그 외(버그로 인한 예상 못한 예외)는 운영 환경에서 원문 대신 해시만 노출해서
            // 내부 정보(DB 연결 문자열 등 예외 메시지에 섞여 나올 수 있는 값) 유출을 막는다.
            var clientMsg = isExpected || Config<GameConfig>.Get().IsShowErrorDetail
                ? errorMsg
                : errorHash;

            var res = new ErrorResponsePacket
            {
                Info = new ResponseInfoPacket
                {
                    ResultCode = errorCode,
                    ResultMsg = clientMsg,
                }
            };

            await ResWriteHelper.WriteResponseBodyAsync(httpContext, res, typeof(ErrorResponsePacket), StatusCodes.Status500InternalServerError);
        }

        private readonly ILogger<ErrorHandler> _logger;
    }
}

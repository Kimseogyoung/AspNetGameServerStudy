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
        public async Task Handle(HttpContext httpContext)
        {
            var exception = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            if (exception == null)
            {
                _logger.Warn("EXCEPTION_IS_NULL");
                return;
            }

            var errorCode = (int)EErrorCode.NO_HANDLING_ERROR;
            var errorMsg = exception.Message;
            var errorHash = HashHelper.CalculateMD5Hash(errorMsg)[..6];
            object errorArgsObj = null;
            if (exception is IServerExpectedException expectedExc)
            {
                errorCode = expectedExc.ErrorCode;
                errorArgsObj = expectedExc.ErrorArgs;
            }

            var errorArgs = errorArgsObj != null
                ? System.Text.Json.JsonSerializer.Serialize(errorArgsObj)
                : "";

            _logger.Error("Error:{Code}:{Hash}:{Msg} Args({Args}) StackTrace({StackTrace})", errorCode, errorHash, errorMsg, errorArgs, exception.StackTrace);

            // TODO: 에러 리포트
            // sentry

            var res = new ErrorResponsePacket
            {
                Info = new ResponseInfoPacket
                {
                    ResultCode = errorCode,
                    ResultMsg = errorMsg,
                }
            };

            await ResWriteHelper.WriteResponseBodyAsync(httpContext, res, typeof(ErrorResponsePacket), StatusCodes.Status500InternalServerError);
        }

        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
    }
}

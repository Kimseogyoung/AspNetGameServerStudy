using Microsoft.AspNetCore.Diagnostics;
using Proto;
using Protocol;
using WebStudyServer.Extension;
using WebStudyServer.Helper;

namespace WebStudyServer
{
    public class ErrorHandler
    {
        public ErrorHandler() 
        {
        }

        public Task Handle(HttpContext httpContext)
        {
            var exc = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
            if (exc == null)
            {
                _logger.Warn("EXCEPTION_IS_NULL");
                return Task.FromResult(false);
            }

            return HandleInternalAsync(httpContext, exc);
        }

        public Task HandleWithException(HttpContext httpContext, Exception exception)
        {
            return HandleInternalAsync(httpContext, exception);
        }

        public async Task<object> HandleWithExceptionAsnyc(HttpContext httpContext, Exception exception)
        {
            return await HandleInternalAsync(httpContext, exception);
        }

        private async Task<object> HandleInternalAsync(HttpContext httpContext, Exception exception)
        {
            dynamic dynArgs = "";
            var errorCode = (int)EErrorCode.NO_HANDLING_ERROR;
            var errorMsg = exception.Message;
            var errorHash = HashHelper.CalculateMD5Hash(errorMsg).Substring(0, 6);
            var errorArgs = "";
            switch (exception)
            {
                case GameException gameExc:
                    errorCode = gameExc.Code;
                    break;
                default:
                    break;
            }

            // TODO: 로그
            _logger.Error("Error:{Code}:{Hash}:{Msg} Args({Args}) StackTrace({StackTrace})", errorCode, errorHash, errorMsg, errorArgs, exception.StackTrace);

            // TODO: 에러 리포트
            // sentry

            var res = new ErrorResponsePacket
            {
                Info = new ResInfoPacket
                {
                    ResultCode = errorCode,
                    ResultMsg = errorMsg,
                }
            };

            await ResWriteHelper.WriteResponseBodyAsync(httpContext, res, typeof(ErrorResponsePacket), StatusCodes.Status500InternalServerError);
            return res;
        }

        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
    }
}

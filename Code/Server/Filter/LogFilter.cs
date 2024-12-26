using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using WebStudyServer.Extension;

namespace WebStudyServer.Filter
{
    public class LogFilter : ActionFilterAttribute
    {
        public LogFilter(ILogger<LogFilter> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpReq = context.HttpContext.Request;
            var args = new Dictionary<string, object>()
            {
                { "Method", httpReq.Method },
                { "Path", httpReq.Path.ToString() },
                { "Body", context.ActionArguments },
            };

            _logger.Info("Req Method({Method}) Path({Path}) Body({Body})", httpReq.Method, httpReq.Path.ToString(), context.ActionArguments);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var httpReq = context.HttpContext.Request;
            var httpRes = context.HttpContext.Response;

            object resBody = null;
            if (context.Result is ObjectResult result)
            {
                resBody = result.Value;
            }

            var args = new Dictionary<string, object>()
            {
                { "Method", httpReq.Method },
                { "Path", httpReq.Path.ToString() },
                { "Code", httpRes.StatusCode },
                { "Body", resBody },
            };

            _logger.Info("Res Method({Method}) Path({Path}) Code({Code}) Body({Body})", httpReq.Method, httpReq.Path, httpRes.StatusCode, resBody);
        }

        private readonly ILogger _logger;
    }
}

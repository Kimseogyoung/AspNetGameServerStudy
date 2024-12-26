using Microsoft.AspNetCore.Mvc.Filters;
using WebStudyServer.Helper;
using WebStudyServer.Repo;

namespace WebStudyServer.Filter
{
    public class AuthFilter : ActionFilterAttribute
    {
        public AuthFilter(RpcContext rpcContext, ILogger<AuthFilter> logger)
        {
            _rpcContext = rpcContext;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ReqHelper.ValidContext(_rpcContext.SessionLoadState == RpcContext.ESessionLoadState.LOADED, "FAILED_SESSION_LOAD", () => new { SessionKey = _rpcContext.SessionKey, SessionLoadState = _rpcContext.SessionLoadState });
            ReqHelper.ValidContext(_rpcContext.AccountId != 0, "NOT_FOUND_ACCOUNT_IN_AUTH_FILTER", () => new { SessionKey= _rpcContext.SessionKey });
        }

        private readonly RpcContext _rpcContext;
        private readonly ILogger _logger;
    }
}

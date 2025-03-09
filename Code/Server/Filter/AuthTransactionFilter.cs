using Microsoft.AspNetCore.Mvc.Filters;
using Server.Repo;
using WebStudyServer.Repo;

namespace WebStudyServer.Filter
{
    public class AuthTransactionFilter : ActionFilterAttribute
    {
        public AuthTransactionFilter(RpcContext rpcContext, DbRepo dbRepo, UserLockService userLockService, ILogger<AuthTransactionFilter> logger)
        {
            _rpcContext = rpcContext;
            _dbRepo = dbRepo;
            _userLockService = userLockService;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ActionExecutedContext executedContext = null;
            await _userLockService.RunAtomicAsync(_rpcContext.AccountId, _dbRepo, async () =>
            {
                executedContext = await next(); // 실제 API Action
            });

            // API 실행 이후 
            if (executedContext == null || executedContext.Exception != null)
            {
                // 롤백
                _dbRepo.Rollback();
                return;
            }

            // 저장
            _dbRepo.Commit();
        }

        private readonly RpcContext _rpcContext;
        private readonly DbRepo _dbRepo;
        private readonly UserLockService _userLockService;
        private readonly ILogger _logger;
    }
}

using Microsoft.AspNetCore.Mvc.Filters;
using Server.Repo;
using WebStudyServer.Repo;

namespace WebStudyServer.Filter
{
    public enum ETransactionType
    {
        NONE,
        AUTH,
        USER,
        CENTER
    }

    public class UserTransactionFilter : ActionFilterAttribute, IEndpointFilter
    {
        public ETransactionType TransactionType { get; set; } = ETransactionType.NONE;

        public UserTransactionFilter(RpcContext rpcContext, DbRepo dbRepo, UserLockService userLockService, ILogger<UserTransactionFilter> logger)
        {
            _rpcContext = rpcContext;
            _userLockService = userLockService;
            _dbRepo = dbRepo;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _dbRepo.BeginOwnUserRepo();

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

        public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            _dbRepo.BeginOwnUserRepo();

            ActionExecutedContext executedContext = null;
            await _userLockService.RunAtomicAsync(_rpcContext.AccountId, _dbRepo, async () =>
            {
                executedContext = (ActionExecutedContext)await next(context); // 실제 API Action
            });

            // API 실행 이후 
            if (executedContext == null || executedContext.Exception != null)
            {
                // 롤백
                _dbRepo.Rollback();
                return Results.Empty;
            }

            // 저장
            _dbRepo.Commit();
            return executedContext;
        }

        private readonly RpcContext _rpcContext;
        private readonly DbRepo _dbRepo;
        private readonly UserLockService _userLockService;
        private readonly ILogger _logger;
    }
}

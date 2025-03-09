using MySqlConnector;
using Server.Repo;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Repo;

namespace WebStudyServer
{
    public class UserLockService
    {
        public UserLockService(RpcContext rpcContext, ILogger<UserLockService> logger)
        {
            _rpcContext = rpcContext;
            _logger = logger;
            _useDbLock = APP.Cfg.UseUserLock;
        }

        public async Task RunAtomicAsync(ulong accountId, DbRepo dbRepo, Func<Task> action)
        {
            if (!_useDbLock 
                || accountId == 0) // 익명 요청은 유저 락을 사용하지 않음
            {
                _logger.Debug("SkipUserLock");
                await action();
                return;
            }

            _logger.Debug("WaitUserLock AccountId({AccountId})", accountId);

            var idParam = new MySqlParameter("@id", $"acnt:{accountId}");
            var timeOutParam = new MySqlParameter("@timeout", APP.Cfg.UserLockTimeoutSpan);
            var getLockResult = dbRepo.Auth.RunCommand<long>("SELECT GET_LOCK(@id, @timeout)", [idParam, timeOutParam]);

            if (getLockResult <= 0) // NOTE: result가 0이면 GetLock에 실패
            {
                _logger.Error("FAILED_GET_USER_LOCK AccountId({AccountId}) Result({Result})", accountId, getLockResult);
                throw new UserLockException(accountId, "USER_LOCK_DB_TIME_OUT");
            }

            try
            {
                _logger.Debug("EnterUserLock AccountId({AccountId})", accountId);
                await action();
            }
            catch (TimeoutException exc)
            {
                throw new UserLockException(accountId, "USER_LOCK_DB_TIME_OUT", exc.Message);
            }
            finally
            {
                _logger.Debug("ExitUserLock AccountId({AccountId})", accountId);
                var releaseLockResult = dbRepo.Auth.RunCommand<long>("SELECT RELEASE_LOCK(@id)", new MySqlParameter[] { idParam });
                if (releaseLockResult <= 0) // NOTE: result가 0이면 ReleaseLock에 실패
                {
                    _logger.Error("FAILED_RELEASE_USER_LOCK AccountId({AccountId}) Result({Result})", accountId, releaseLockResult);
                    throw new UserLockException(accountId, "FAILED_RELEASE_USER_LOCK");
                }
            }
        }

        private readonly RpcContext _rpcContext;
        private readonly bool _useDbLock;
        private readonly ILogger _logger;
    }
}

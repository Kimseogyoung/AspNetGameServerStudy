using ServerCore;
using ServerCore.Extension;

namespace WebStudyServer
{
    public class UserLockService
    {
        public UserLockService(ILockService lockService, ILogger<UserLockService> logger)
        {
            _lockService = lockService;
            _logger = logger;
            _useDbLock = Config<CoreConfig>.Get().UseUserLock;
        }

        public async Task RunAtomicAsync(ulong accountId, Func<Task> action)
        {
            if (!_useDbLock || accountId == 0) // 익명 요청은 유저 락을 사용하지 않음
            {
                _logger.Debug("SkipUserLock");
                await action();
                return;
            }

            _logger.Debug("WaitUserLock AccountId({AccountId})", accountId);

            if (!await _lockService.EnterAsync(accountId))
            {
                _logger.Error("FAILED_GET_USER_LOCK AccountId({AccountId})", accountId);
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
                if (!await _lockService.ExitAsync(accountId))
                {
                    _logger.Error("FAILED_RELEASE_USER_LOCK AccountId({AccountId})", accountId);
                    throw new UserLockException(accountId, "FAILED_RELEASE_USER_LOCK");
                }
            }
        }

        private readonly ILockService _lockService;
        private readonly bool _useDbLock;
        private readonly ILogger _logger;
    }
}

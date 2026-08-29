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

            // 해제 실패도 오류로 올린다. 다만 그 예외가 진행 중이던 예외를 그냥 덮으면
            // 원인을 잃으므로, 붙잡아 두었다가 InnerException 으로 실어보낸다.
            Exception pending = null;
            try
            {
                _logger.Debug("EnterUserLock AccountId({AccountId})", accountId);
                await action();
            }
            catch (TimeoutException exc)
            {
                pending = new UserLockException(accountId, "USER_LOCK_DB_TIME_OUT", exc.Message, exc);
                throw pending;
            }
            catch (Exception exc)
            {
                pending = exc;
                throw;
            }
            finally
            {
                _logger.Debug("ExitUserLock AccountId({AccountId})", accountId);
                if (!await _lockService.ExitAsync(accountId))
                {
                    _logger.Error(pending, "FAILED_RELEASE_USER_LOCK AccountId({AccountId})", accountId);
                    throw new UserLockException(
                        accountId, "FAILED_RELEASE_USER_LOCK", pending?.Message ?? string.Empty, pending);
                }
            }
        }

        private readonly ILockService _lockService;
        private readonly bool _useDbLock;
        private readonly ILogger _logger;
    }
}

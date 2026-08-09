using Server.Repo;
using ServerCore;

namespace WebStudyServer
{
    // MySQL GET_LOCK / RELEASE_LOCK 기반 분산 락 구현.
    // GET_LOCK: timeout(초) 내에 락 획득 시 1, 실패 시 0, 오류 시 null 반환.
    public class MySqlLockService : ILockService
    {
        private readonly GlobalDbRepo _dbRepo;

        public MySqlLockService(GlobalDbRepo dbRepo)
        {
            _dbRepo = dbRepo;
        }

        public async Task<bool> EnterAsync(ulong accountId)
        {
            var result = await _dbRepo.Auth.Repository.Db.ExecuteAsync<long>(
                db => db.QuerySingle<long>(
                    "SELECT GET_LOCK(@id, @timeout)",
                    new { id = $"acnt:{accountId}", timeout = Config<CoreConfig>.Get().UserLockTimeoutSpan.TotalSeconds }));
            return result > 0;
        }

        public async Task<bool> ExitAsync(ulong accountId)
        {
            var result = await _dbRepo.Auth.Repository.Db.ExecuteAsync<long>(
                db => db.QuerySingle<long>(
                    "SELECT RELEASE_LOCK(@id)",
                    new { id = $"acnt:{accountId}" }));
            return result > 0;
        }
    }
}

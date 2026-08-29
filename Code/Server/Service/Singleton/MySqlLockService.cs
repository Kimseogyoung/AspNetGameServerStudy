using Dapper;
using ServerCore;
using ServerCore.Repo.Database;

namespace WebStudyServer
{
    // MySQL GET_LOCK / RELEASE_LOCK 기반 분산 락 구현.
    // GET_LOCK: timeout(초) 내에 락 획득 시 1, 실패 시 0, 오류 시 null 반환.
    //
    // 요청 트랜잭션과 분리된 전용 커넥션을 쓴다 — DbSessionManager가 추적하는 세션은
    // 커밋 시점에 일괄로 닫히므로, 커밋 뒤에 일어나는 락 해제를 거기에 얹을 수 없다.
    public class MySqlLockService : ILockService, IDisposable
    {
        public async Task<bool> EnterAsync(ulong accountId)
        {
            _connection ??= await DbUtilityConnection.OpenAsync(GetConnectionStr());

            // GET_LOCK 은 오류 시 NULL 이다. long 으로 받으면 Dapper 가 매핑에 실패해 던진다.
            var result = await _connection.ExecuteAsync(conn => conn.QuerySingleAsync<long?>(
                "SELECT GET_LOCK(@id, @timeout)",
                new { id = MakeKey(accountId), timeout = Config<CoreConfig>.Get().UserLockTimeoutSpan.TotalSeconds }));

            return result > 0;
        }

        public async Task<bool> ExitAsync(ulong accountId)
        {
            if (_connection == null)
            {
                return true; // Enter가 성공한 적이 없으므로 잡고 있는 락도 없다
            }

            try
            {
                // RELEASE_LOCK 은 이 커넥션이 락을 안 잡고 있으면 NULL 이다.
                var result = await _connection.ExecuteAsync(conn => conn.QuerySingleAsync<long?>(
                    "SELECT RELEASE_LOCK(@id)",
                    new { id = MakeKey(accountId) }));

                return result > 0;
            }
            finally
            {
                Close();
            }
        }

        // Enter 실패로 ExitAsync가 호출되지 않는 경로의 백스톱. Scoped 등록이라 DI가 호출한다.
        public void Dispose()
        {
            Close();
        }

        private static string MakeKey(ulong accountId)
        {
            return $"acnt:{accountId}";
        }

        private static string GetConnectionStr()
        {
            var connList = Config<CoreConfig>.Get().AuthDbConnectionStrList;
            if (connList.Count == 0)
            {
                throw new InvalidOperationException("MySqlLockService requires an Auth DB connection but AuthDbConnectionStrList is empty");
            }

            return connList[0];
        }

        private void Close()
        {
            _connection?.Dispose();
            _connection = null;
        }

        private DbUtilityConnection _connection;
    }
}

using System.Data;
using MySqlConnector;

namespace ServerCore.Repo.Database
{
    // 요청 작업 단위(DbSessionManager)에 참여하지 않는 독립 커넥션.
    // 구분 기준은 트랜잭션 유무가 아니라 "요청 커밋과 수명을 공유하는가"다 —
    // 매니저가 추적하는 세션은 커밋 시 일괄로 닫히므로,
    // 커밋보다 오래 살아야 하는 작업(분산 락 등)은 여기를 쓴다.
    // 커밋/롤백이 없으므로 엔티티 쓰기에는 쓰지 않는다.
    public sealed class DbUtilityConnection : IDisposable
    {
        // 반환된 인스턴스는 항상 열린 상태다. 실패 시 커넥션을 정리하고 던진다.
        public static async Task<DbUtilityConnection> OpenAsync(string connectionStr)
        {
            var connection = new MySqlConnection(connectionStr);
            try
            {
                await connection.OpenAsync();
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }

            return new DbUtilityConnection(connection);
        }

        private IDbConnection _connection;

        private DbUtilityConnection(IDbConnection connection)
        {
            _connection = connection;
        }

        public Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> func)
        {
            ObjectDisposedException.ThrowIf(_connection == null, this);

            return func(_connection);
        }

        // 커넥션 종료 자체가 그 커넥션이 잡고 있던 MySQL 락을 해제한다.
        public void Close()
        {
            if (_connection == null)
            {
                return;
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}

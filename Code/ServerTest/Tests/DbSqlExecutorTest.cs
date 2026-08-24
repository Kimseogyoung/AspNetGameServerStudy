using ServerCore.Repo.Database;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// DBSqlExecutor 정리 경로 테스트 (DB 서버 없이 돈다)
    /// - 생성자는 커넥션을 열지 않으므로 Open 없이 정리 경로만 태울 수 있다.
    /// - Commit/Rollback 뒤에 Close 가 한 번 더 불려도 안전해야 한다.
    /// </summary>
    public class DbSqlExecutorTest
    {
        private const string ConnectionStr = "Server=127.0.0.1;Database=none;Uid=none;Pwd=none;";

        [Fact]
        public void CommitThenClose_DoesNotThrow()
        {
            var executor = new DBSqlExecutor(ConnectionStr);

            executor.Commit();
            executor.Close();
        }

        [Fact]
        public void RollbackThenClose_DoesNotThrow()
        {
            var executor = new DBSqlExecutor(ConnectionStr);

            executor.Rollback();
            executor.Close();
        }

        [Fact]
        public void CloseTwice_DoesNotThrow()
        {
            var executor = new DBSqlExecutor(ConnectionStr);

            executor.Close();
            executor.Close();
        }
    }
}

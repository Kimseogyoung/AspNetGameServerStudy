using System.Data;

namespace WebStudyServer.Repo.Database
{
    // Singleton — 연결 문자열을 받아 MySQL 트랜잭션 팩토리를 생성한다.
    public class MySqlDbSessionFactory : IDbSessionFactory
    {
        public IDbSession Create(string connectionString)
        {
            var executor = DBSqlExecutor.StartTransaction(connectionString, IsolationLevel.ReadCommitted);
            return new DapperDbSession(executor);
        }
    }
}

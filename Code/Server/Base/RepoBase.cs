using System.Data;
using MySqlConnector;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class RepoBase
    {
        public int ShardId { get; private set; }
        protected abstract void PrepareComp();

        // TODO(Step5): Component 전환 완료 후 _executor 제거
        protected DBSqlExecutor _executor = null!;
        protected IDbExecutorFactory _dbFactory = null!;

        public void Init(int shardId, IDbExecutorFactory dbFactory)
        {
            ShardId = shardId;
            _dbFactory = dbFactory;
            if (dbFactory is DapperExecutorFactory dapper)
            {
                _executor = dapper.RawExecutor;
            }

            PrepareComp();
        }

        public T RunCommand<T>(string commandText, params MySqlParameter[] parameters)
        {
            return _executor.Excute((sqlConnection, transaction) =>
            {
                using var command = sqlConnection.CreateCommand();
                command.Transaction = transaction;
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;

                // 파라미터 추가
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                return (T)command.ExecuteScalar();
            });
        }
    }
}

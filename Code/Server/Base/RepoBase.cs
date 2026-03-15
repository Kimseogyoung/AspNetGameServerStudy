using System.Data;
using MySqlConnector;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class RepoBase
    {
        public int ShardId { get; private set; }
        protected abstract void PrepareComp();

        protected IDbSession _dbFactory = null!;

        public void Init(int shardId, IDbSession dbFactory)
        {
            ShardId = shardId;
            _dbFactory = dbFactory;
            PrepareComp();
        }

        public T RunCommand<T>(string commandText, params MySqlParameter[] parameters)
        {
            if (_dbFactory is not DapperDbSession dapper)
                throw new NotSupportedException("RunCommand는 MySQL 모드에서만 지원됩니다.");

            return dapper.RawExecutor.Excute((sqlConnection, transaction) =>
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

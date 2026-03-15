using System.Data;
using MySqlConnector;
using Server.Repo.Database;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class RepoBase
    {
        public int ShardId { get; private set; }
        protected abstract void PrepareComp();

        protected IRepository _repository = null!;

        public RepoBase(int shardId, IRepository repository)
        {
            ShardId = shardId;
            _repository = repository;
            PrepareComp();
        }

        public T RunCommand<T>(string commandText, params MySqlParameter[] parameters)
        {
            if (_repository.Db is not DapperDbSession dapper)
                throw new NotSupportedException("RunCommand는 MySQL 모드에서만 지원됩니다.");

            return dapper.RawExecutor.Excute((sqlConnection, transaction) =>
            {
                using var command = sqlConnection.CreateCommand();
                command.Transaction = transaction;
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;

                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                var scalar = command.ExecuteScalar();
                return (T)Convert.ChangeType(scalar, typeof(T));
            });
        }
    }
}

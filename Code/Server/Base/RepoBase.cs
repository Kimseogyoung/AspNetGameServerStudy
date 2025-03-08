using Microsoft.AspNetCore.Components;
using MySqlConnector;
using System.Data;
using WebStudyServer.Extension;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class RepoBase
    {
        public int ShardId { get; private set; }
        protected abstract void PrepareComp();

        protected DBSqlExecutor _executor = null!;

        public void Init(int shardId, DBSqlExecutor executor)
        {
            ShardId = shardId;
            _executor = executor;

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

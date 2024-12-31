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
        protected DBSqlExecutor _executor { get; private set; } = null!;
        protected abstract List<string> _dbConnStrList { get; }
        protected abstract void PrepareComp();

        public void Init(int shardId)
        {
            var dbConnectionStr = GetDbConnectionStr();
            ShardId = shardId;
            _executor = DBSqlExecutor.Create(dbConnectionStr, System.Data.IsolationLevel.ReadCommitted);

            PrepareComp();
        }


        public void Commit()
        {
            _executor.Commit();
        }

        public void Rollback()
        {
            _executor.Rollback();
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

        private string GetDbConnectionStr()
        {
            if(_shardCnt <= ShardId)
            {
                throw new Exception("dd");
            }

            return _dbConnStrList[ShardId];
        }
        private int _shardCnt => _dbConnStrList.Count;
    }
}

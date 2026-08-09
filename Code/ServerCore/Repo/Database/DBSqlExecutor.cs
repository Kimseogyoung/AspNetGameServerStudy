using System.Data;
using System.Data.Common;
using MySqlConnector;

namespace ServerCore.Repo.Database
{
    public class DBSqlExecutor
    {
        public static DBSqlExecutor StartTransaction(string connectionStr, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var excutor = new DBSqlExecutor(connectionStr);
            excutor.Open(isolationLevel);
            return excutor;
        }

        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public DBSqlExecutor(string connectionStr)
        {
            _connection = new MySqlConnection(connectionStr);
            _transaction = null;
        }

        public void Open(IsolationLevel isolationLevel)
        {
            _connection.Open();
            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        public async Task ExecuteAsync(Func<IDbConnection, IDbTransaction, Task> func)
        {
            await func.Invoke(_connection, _transaction);
        }

        public async Task<T> ExecuteAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> func)
        {
            return await func.Invoke(_connection, _transaction);
        }

        public void Commit()
        {
            _transaction?.Commit();

            CloseInternal();
        }

        public void Rollback()
        {
            _transaction?.Rollback();

            CloseInternal();
        }

        public void Close()
        {
            CloseInternal();
        }

        private void CloseInternal()
        {
            _transaction?.Dispose();

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}

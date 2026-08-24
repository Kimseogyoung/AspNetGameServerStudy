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

        // 커밋/롤백이 던져도 커넥션은 닫는다.
        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            finally
            {
                CloseInternal();
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                CloseInternal();
            }
        }

        public void Close()
        {
            CloseInternal();
        }

        // 여러 번 불려도 한 번만 정리한다.
        private void CloseInternal()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;

            _transaction?.Dispose();
            _transaction = null;

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        private bool _closed;
    }
}

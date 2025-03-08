using MySqlConnector;
using System.Data;
using System.Data.Common;

namespace WebStudyServer.Repo.Database
{
    public class DBSqlExecutor 
    {
        public static DBSqlExecutor StartTransaction(string connectionStr, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var excutor = new DBSqlExecutor(connectionStr);
            excutor.Open(isolationLevel);
            return excutor;
        }

        private IDbConnection _connection;
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

        public void Excute(Action<IDbConnection, IDbTransaction> func)
        {
            func.Invoke(_connection, _transaction);
        }

        public T Excute<T>(Func<IDbConnection, IDbTransaction, T> func)
        {
            return func.Invoke(_connection, _transaction);
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }

            CloseInternal();
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }

            CloseInternal();
        }

        public void Close()
        {
            CloseInternal();
        }

        private void CloseInternal()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}

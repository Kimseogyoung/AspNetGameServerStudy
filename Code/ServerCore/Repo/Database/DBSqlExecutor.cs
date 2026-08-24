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
            try
            {
                await func.Invoke(_connection, _transaction);
            }
            catch (MySqlException e) when (IsTransactionFatal(e))
            {
                _transactionDead = true;
                throw;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> func)
        {
            try
            {
                return await func.Invoke(_connection, _transaction);
            }
            catch (MySqlException e) when (IsTransactionFatal(e))
            {
                _transactionDead = true;
                throw;
            }
        }

        // 서버가 트랜잭션을 통째로 롤백하는 오류. 문장 하나만 실패하는 오류와 구분해야 한다.
        // 락 대기 타임아웃은 innodb_rollback_on_timeout 이 켜져 있을 때만 여기 해당하는데,
        // 기본값이 OFF 라 문장만 롤백되고 트랜잭션은 살아 있다. 그래서 데드락만 본다.
        private static bool IsTransactionFatal(MySqlException e)
        {
            return e.ErrorCode == MySqlErrorCode.LockDeadlock;
        }

        // 커밋/롤백이 던져도 커넥션은 닫는다.
        public void Commit()
        {
            // 서버가 이미 롤백했으므로 COMMIT 은 no-op 으로 성공한다. 그대로 두면 트랜잭션이
            // 날아간 것이 성공으로 보이고, 롤백 뒤에 실행된 문장은 autocommit 으로 남는다.
            if (_transactionDead)
            {
                Rollback();
                throw new InvalidOperationException("DEAD_TRANSACTION_CANNOT_COMMIT");
            }

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
        private bool _transactionDead;
    }
}

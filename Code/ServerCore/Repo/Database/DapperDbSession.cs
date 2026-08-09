namespace ServerCore.Repo.Database
{
    // DBSqlExecutor(트랜잭션 라이프사이클)를 IDbSession으로 래핑한다.
    // Execute 호출마다 DapperDbExecutor 인스턴스를 생성하지만,
    // 내부 conn/tx는 DBSqlExecutor가 보유하므로 재연결 비용은 없다.
    public class DapperDbSession : IDbSession
    {
        private readonly DBSqlExecutor _executor;

        public DapperDbSession(DBSqlExecutor executor)
        {
            _executor = executor;
        }

        public Task ExecuteAsync(Func<IDbExecutor, Task> action)
        {
            return _executor.Excute((conn, tx) => action(new DapperDbExecutor(conn, tx)));
        }

        public Task<T> ExecuteAsync<T>(Func<IDbExecutor, Task<T>> func)
        {
            return _executor.Excute((conn, tx) => func(new DapperDbExecutor(conn, tx)));
        }

        public void Commit()
        {
            _executor.Commit();
        }

        public void Rollback()
        {
            _executor.Rollback();
        }

        public void Close()
        {
            _executor.Close();
        }
    }
}

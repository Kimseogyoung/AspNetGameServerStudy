namespace WebStudyServer.Repo.Database
{
    // DBSqlExecutor(트랜잭션 라이프사이클)를 IDbExecutorFactory로 래핑한다.
    // Execute 호출마다 DapperDbExecutor 인스턴스를 생성하지만,
    // 내부 conn/tx는 DBSqlExecutor가 보유하므로 재연결 비용은 없다.
    public class DapperExecutorFactory : IDbExecutorFactory
    {
        private readonly DBSqlExecutor _executor;

        // RepoBase 과도기 전환용 — Step5 완료 후 제거
        internal DBSqlExecutor RawExecutor => _executor;

        public DapperExecutorFactory(DBSqlExecutor executor)
        {
            _executor = executor;
        }

        public void Execute(Action<IDbExecutor> action)
        {
            _executor.Excute((conn, tx) => action(new DapperDbExecutor(conn, tx)));
        }

        public T Execute<T>(Func<IDbExecutor, T> func)
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
    }
}

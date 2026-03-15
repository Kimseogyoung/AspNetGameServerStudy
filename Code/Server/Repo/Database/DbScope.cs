namespace WebStudyServer.Repo.Database
{
    // Scoped — 요청 단위로 열린 IDbSession를 추적하고 트랜잭션 라이프사이클을 관리한다.
    // IDbSessionFactory(Singleton)가 생성을, DbScope(Scoped)가 추적/커밋을 담당한다.
    public class DbScope
    {
        private readonly IDbSessionFactory _sessionFactory;
        private readonly Dictionary<string, IDbSession> _open = [];

        public DbScope(IDbSessionFactory connectionFactory)
        {
            _sessionFactory = connectionFactory;
        }

        public IDbSession Open(string connectionString)
        {
            if (!_open.TryGetValue(connectionString, out var session))
            {
                session = _sessionFactory.Create(connectionString);
                _open[connectionString] = session;
            }

            return session;
        }

        public void Commit()
        {
            foreach (var factory in _open.Values)
            {
                factory.Commit();
            }

            _open.Clear();
        }

        public void Rollback()
        {
            foreach (var factory in _open.Values)
            {
                factory.Rollback();
            }

            _open.Clear();
        }

        public void Close()
        {
            foreach (var factory in _open.Values)
            {
                factory.Close();
            }

            _open.Clear();
        }
    }
}

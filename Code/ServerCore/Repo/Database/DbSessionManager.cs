namespace ServerCore.Repo.Database
{
    // Scoped — 요청 단위로 열린 IDbSession를 추적하고 트랜잭션 라이프사이클을 관리한다.
    // IDbSessionFactory(Singleton)가 생성을, DbScope(Scoped)가 추적/커밋을 담당한다.
    public class DbSessionManager
    {
        private readonly IDbSessionFactory _sessionFactory;
        private readonly Dictionary<string, IDbSession> _openSession = [];

        public DbSessionManager(IDbSessionFactory connectionFactory)
        {
            _sessionFactory = connectionFactory;
        }

        public IDbSession Open(string connectionString)
        {
            if (!_openSession.TryGetValue(connectionString, out var session))
            {
                session = _sessionFactory.Create(connectionString);
                _openSession[connectionString] = session;
            }

            return session;
        }

        public void Commit()
        {
            try
            {
                foreach (var session in _openSession.Values)
                {
                    session.Commit();
                }
            }
            finally
            {
                _openSession.Clear();
            }
        }

        public void Rollback()
        {
            try
            {
                foreach (var session in _openSession.Values)
                {
                    session.Rollback();
                }
            }
            finally
            {
                _openSession.Clear();
            }
        }

        public void Close()
        {
            try
            {
                foreach (var session in _openSession.Values)
                {
                    session.Close();
                }
            }
            finally
            {
                _openSession.Clear();
            }
        }
    }
}

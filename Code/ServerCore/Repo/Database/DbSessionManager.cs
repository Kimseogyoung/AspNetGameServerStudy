namespace ServerCore.Repo.Database
{
    // Scoped — 요청 단위 IDbSession을 커넥션 문자열로 캐시한다. 실제 커넥션은 첫 쿼리에서 열린다.
    // 커밋/롤백 순서는 GameDb가 정하고, 세션을 닫는 것은 여기가 맡는다.
    // 커밋도 롤백도 안 탄 세션은 스코프 종료의 Dispose가 닫는다.
    public class DbSessionManager : IDisposable
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
                session = new LazyDbSession(() => _sessionFactory.Create(connectionString));
                _openSession[connectionString] = session;
            }

            return session;
        }

        // 도중에 던져도 남은 세션을 닫고 나간다.
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
                Close();
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
                Close();
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

        public void Dispose()
        {
            Close();
        }
    }
}

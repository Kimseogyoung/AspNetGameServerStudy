namespace ServerCore.Repo.Database
{
    // 첫 쿼리까지 실제 세션 생성을 미룬다. 캐시만 맞고 끝나는 요청은 커넥션도 트랜잭션도 열지 않는다.
    // 요청 스코프 안에서 순차로만 쓰이므로 생성에 락을 두지 않는다.
    internal sealed class LazyDbSession : IDbSession
    {
        public LazyDbSession(Func<IDbSession> factory)
        {
            _factory = factory;
        }

        public Task ExecuteAsync(Func<IDbExecutor, Task> action)
        {
            return Materialize().ExecuteAsync(action);
        }

        public Task<T> ExecuteAsync<T>(Func<IDbExecutor, Task<T>> func)
        {
            return Materialize().ExecuteAsync(func);
        }

        public void Commit()
        {
            _session?.Commit();
        }

        public void Rollback()
        {
            _session?.Rollback();
        }

        public void Close()
        {
            _session?.Close();
        }

        private IDbSession Materialize()
        {
            return _session ??= _factory();
        }

        private readonly Func<IDbSession> _factory;
        private IDbSession _session;
    }
}

namespace ServerCore.Repo.Database
{
    // IDbSession의 InMemory 구현.
    // Execute 호출마다 InMemoryDbExecutor를 생성한다.
    // Commit/Rollback은 no-op (InMemory는 트랜잭션 없음).
    public class InMemoryDbSession : IDbSession
    {
        private readonly InMemoryStore _store;

        public InMemoryDbSession(InMemoryStore store)
        {
            _store = store;
        }

        public Task ExecuteAsync(Func<IDbExecutor, Task> action)
        {
            return action(new InMemoryDbExecutor(_store));
        }

        public Task<T> ExecuteAsync<T>(Func<IDbExecutor, Task<T>> func)
        {
            return func(new InMemoryDbExecutor(_store));
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public void Close()
        {
        }
    }
}

namespace ServerCore.Repo.Database
{
    // Singleton — 연결 문자열을 무시하고 공유 InMemoryStore 기반 팩토리를 생성한다.
    public class InMemoryDbSessionFactory : IDbSessionFactory
    {
        private readonly InMemoryStore _store;

        public InMemoryDbSessionFactory(InMemoryStore store)
        {
            _store = store;
        }

        public IDbSession Create(string connectionString)
        {
            return new InMemoryDbSession(_store);
        }
    }
}

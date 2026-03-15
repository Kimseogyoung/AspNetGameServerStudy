using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    // IRepository의 InMemory 구현.
    // InMemoryStore가 영속 저장소이므로 Cache → DB fallback 없이 DbExecutor에 직접 위임한다.
    public class InMemoryRepository : IRepository
    {
        public ICacheSession Cache { get; }
        public IDbSession Db { get; }

        public InMemoryRepository(ICacheSession cache, IDbSession dbFactory)
        {
            Cache = cache;
            Db = dbFactory;
        }

        public T Get<T>(CacheKey key, Func<IDbExecutor, T> dbFetch) where T : ModelBase
        {
            return Db.Execute(db => dbFetch(db));
        }

        public List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, CacheKey> keySelector) where T : ModelBase
        {
            return Db.Execute(db => dbFetch(db));
        }

        public List<T> GetListFiltered<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, bool> predicate) where T : ModelBase
        {
            return Db.Execute(db => dbFetch(db)).Where(predicate).ToList();
        }

        public T Insert<T>(T entity, Func<T, CacheKey> keyFactory) where T : ModelBase
        {
            return Db.Execute(db => db.Insert<T>(entity));
        }

        public void Update<T>(T entity, CacheKey key) where T : ModelBase
        {
            Db.Execute(db => db.Update<T>(entity));
        }
    }
}

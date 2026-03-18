using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    // IRepository의 InMemory 구현.
    // InMemoryStore가 영속 저장소이므로 Cache → DB fallback 없이 DbExecutor에 직접 위임.
    public class InMemoryRepository : IRepository
    {
        public ICacheSession Cache { get; }
        public IDbSession Db { get; }

        public InMemoryRepository(ICacheSession cache, IDbSession dbFactory)
        {
            Cache = cache;
            Db = dbFactory;
        }

        public List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase
        {
            return Db.Execute(dbFetch);
        }

        public T Insert<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            return Db.Execute(db => db.Insert<T>(entity));
        }

        public void Update<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase
        {
            Db.Execute(db => db.Update<T>(entity));
        }
    }
}

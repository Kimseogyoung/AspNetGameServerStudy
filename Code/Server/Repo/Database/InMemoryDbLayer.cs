using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    // IDbLayer의 InMemory 구현.
    // InMemoryStore가 영속 저장소이므로 Cache → DB fallback 없이 DbFactory에 직접 위임한다.
    public class InMemoryDbLayer : IDbLayer
    {
        public ICacheLayer Cache { get; }
        public IDbSession DbFactory { get; }

        public InMemoryDbLayer(IDbSession dbFactory, ICacheLayer cache)
        {
            DbFactory = dbFactory;
            Cache = cache;
        }

        public T Get<T>(CacheKey key, Func<IDbExecutor, T> dbFetch) where T : ModelBase
        {
            return DbFactory.Execute(db => dbFetch(db));
        }

        public List<T> GetListByPlayerId<T>(CacheKey listKey, ulong playerId, Func<T, CacheKey> keySelector) where T : ModelBase
        {
            return DbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).ToList());
        }

        public List<T> GetListByPlayerIdAndPredicate<T>(CacheKey key, ulong playerId, Func<T, bool> predicate) where T : ModelBase
        {
            return DbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).Where(predicate).ToList());
        }

        public T Insert<T>(T entity, CacheKey key) where T : ModelBase
        {
            return DbFactory.Execute(db => db.Insert<T>(entity));
        }

        public void Update<T>(T entity, CacheKey key) where T : ModelBase
        {
            DbFactory.Execute(db => db.Update<T>(entity));
        }
    }
}

using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    public class SqlRepository : IRepository
    {
        public ICacheSession Cache { get; }
        public IDbSession Db { get; }

        public SqlRepository(ICacheSession cache, IDbSession dbFactory)
        {
            Cache = cache;
            Db = dbFactory;
        }

        // ── SelectList: Cache → DB(dbFetch 위임) → BulkSet ────────────────
        public List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase
        {
            var cached = Cache.GetList<T>(listKey);
            if (cached != null)
            {
                return [.. cached];
            }
            var result = Db.Execute(dbFetch);
            Cache.BulkSet(listKey, result);
            return result;
        }

        // ── Insert: DB → Cache.Set(listKey, entity, match=none → list.Add) ─
        public T Insert<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            entity = Db.Execute(db => db.Insert<T>(entity));
            Cache.Set<T>(listKey, entity, _ => false);
            return entity;
        }

        // ── Update: DB → Cache.Set(listKey, entity, match) ────────────────
        public void Update<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase
        {
            Db.Execute(db => db.Update<T>(entity));
            Cache.Set(listKey, entity, match);
        }
    }
}

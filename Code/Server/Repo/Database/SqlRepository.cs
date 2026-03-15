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

        // ── Select: Cache → DB fallback → Cache Set ───────────
        public T? Get<T>(CacheKey key, Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            var hit = Cache.Get<T>(key);
            if (hit != null)
            {
                return hit;
            }

            var result = Db.Execute(db => dbFetch(db));

            if (result != null)
            {
                Cache.Set(key, result);
            }

            return result;
        }

        // ── SelectList: Cache → DB(dbFetch 위임) → BulkSet ────
        public List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, CacheKey> keySelector) where T : ModelBase
        {
            var cached = Cache.GetList<T>(listKey);
            if (cached != null)
            {
                return [.. cached];
            }

            var result = Db.Execute(db => dbFetch(db));
            Cache.BulkSet(result, keySelector);
            return [.. result];
        }

        // ── SelectList + predicate: Cache → filter / DB → filter (캐시 Set 안함) ─
        public List<T> GetListFiltered<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, bool> predicate) where T : ModelBase
        {
            var cached = Cache.GetList<T>(listKey);
            if (cached != null)
            {
                return [.. cached.Where(predicate)];
            }

            var result = Db.Execute(db => dbFetch(db));
            return [.. result.Where(predicate)];
        }

        // ── Insert: DB → Cache ─────────────────────────────────
        public T Insert<T>(T entity, Func<T, CacheKey> keyFactory) where T : ModelBase
        {
            entity = Db.Execute(db => db.Insert<T>(entity));
            Cache.Set(keyFactory(entity), entity);
            return entity;
        }

        // ── Update: DB → Cache ─────────────────────────────────
        public void Update<T>(T entity, CacheKey key) where T : ModelBase
        {
            Db.Execute(db => db.Update<T>(entity));
            Cache.Set(key, entity);
        }
    }
}

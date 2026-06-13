using ServerCore;
using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
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

        // ── SelectList: Cache → DB(dbFetch 위임) → Set ───────────────────
        public List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase
        {
            if (Cache.TryGet<List<T>>(listKey, out var cached, CacheTtl))
            {
                return [.. cached];
            }

            var result = Db.Execute(dbFetch);
            Cache.Set(listKey, result, CacheTtl);
            return result;
        }

        // ── Insert: DB → 캐시 로드 중이면 항목 추가 ─────────────────────
        public T Insert<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            entity = Db.Execute(db => db.Insert<T>(entity));

            if (Cache.TryGet<List<T>>(listKey, out var cached))
            {
                Cache.Set(listKey, new List<T>(cached) { entity }, CacheTtl);
            }

            return entity;
        }

        // ── Update: DB → 캐시 로드 중이면 match 항목 교체 ────────────────
        public void Update<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase
        {
            Db.Execute(db => db.Update<T>(entity));

            if (!Cache.TryGet<List<T>>(listKey, out var cached))
            {
                return;
            }

            var newList = cached.ToList();
            var idx = newList.FindIndex(x => match(x));
            if (idx >= 0)
            {
                newList[idx] = entity;
            }
            else
            {
                newList.Add(entity);
            }
            Cache.Set(listKey, newList, CacheTtl);
        }

        private static TimeSpan CacheTtl => Core.Cfg.CacheDefaultTtl;
    }
}

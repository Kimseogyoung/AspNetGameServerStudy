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
        public async Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            var cached = await Cache.TryGetAsync<List<T>>(listKey, CacheTtl);
            if (cached.Hit)
            {
                return [.. cached.Value!];
            }

            var result = await Db.ExecuteAsync(dbFetch);
            await Cache.SetAsync(listKey, result, CacheTtl);
            return result;
        }

        // ── Insert: DB → 캐시 로드 중이면 항목 추가 ─────────────────────
        public async Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            entity = await Db.ExecuteAsync(db => db.InsertAsync<T>(entity));

            var cached = await Cache.TryGetAsync<List<T>>(listKey);
            if (cached.Hit)
            {
                await Cache.SetAsync(listKey, new List<T>(cached.Value!) { entity }, CacheTtl);
            }

            return entity;
        }

        // ── Update: DB → 캐시 로드 중이면 match 항목 교체 ────────────────
        public async Task UpdateAsync<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase
        {
            await Db.ExecuteAsync(db => db.UpdateAsync<T>(entity));

            var cached = await Cache.TryGetAsync<List<T>>(listKey);
            if (!cached.Hit)
            {
                return;
            }

            var newList = cached.Value!.ToList();
            var idx = newList.FindIndex(x => match(x));
            if (idx >= 0)
            {
                newList[idx] = entity;
            }
            else
            {
                newList.Add(entity);
            }
            await Cache.SetAsync(listKey, newList, CacheTtl);
        }

        private static TimeSpan CacheTtl => Config<CoreConfig>.Get().CacheDefaultTtl;
    }
}

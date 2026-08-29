using ServerCore;
using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
{
    public class SqlRepository : IRepository
    {
        public SqlRepository(ICacheSession cache, IDbSession dbSession)
        {
            _cache = cache;
            _db = dbSession;
        }

        // ── SelectList: Cache → DB(dbFetch 위임) → Set ───────────────────
        public async Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            var cached = await _cache.TryGetAsync<List<T>>(listKey, CacheTtl);
            if (cached.Hit)
            {
                return [.. cached.Value!];
            }

            var result = await _db.ExecuteAsync(dbFetch);
            await _cache.SetAsync(listKey, result, CacheTtl);

            // 캐시에 넣은 것과 같은 인스턴스를 내주면 호출부의 변형이 캐시에 새어든다.
            // InMemory 캐시 계층은 객체를 참조로 들고 있다.
            return [.. result];
        }

        // ── Insert: DB → 캐시 로드 중이면 항목 추가 ─────────────────────
        public async Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            entity = await _db.ExecuteAsync(db => db.InsertAsync<T>(entity));

            var cached = await _cache.TryGetAsync<List<T>>(listKey);
            if (cached.Hit)
            {
                await _cache.SetAsync(listKey, new List<T>(cached.Value!) { entity }, CacheTtl);
            }

            return entity;
        }

        // ── Insert(캐시 없음): 캐시를 안 지난다 ──────────────────────────
        public Task<T> InsertAsync<T>(T entity) where T : ModelBase
        {
            return _db.ExecuteAsync(db => db.InsertAsync<T>(entity));
        }

        // ── Update: DB → 캐시 로드 중이면 PK 가 같은 항목 교체 ───────────
        public async Task UpdateAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            await _db.ExecuteAsync(db => db.UpdateAsync<T>(entity));

            var cached = await _cache.TryGetAsync<List<T>>(listKey);
            if (!cached.Hit)
            {
                return;
            }

            var newList = cached.Value!.ToList();
            var idx = newList.FindIndex(x => x.PkEquals(entity));
            if (idx >= 0)
            {
                newList[idx] = entity;
            }
            else
            {
                newList.Add(entity);
            }
            await _cache.SetAsync(listKey, newList, CacheTtl);
        }

        // ── UpsertList: DB 업서트 한 문장 → 캐시 리스트 한 번 갱신 ───────
        public async Task UpsertListAsync<T>(IReadOnlyList<T> entityList, CacheKey listKey) where T : ModelBase
        {
            if (entityList.Count == 0)
            {
                return;
            }

            await _db.ExecuteAsync(db => db.UpsertListAsync(entityList));

            var cached = await _cache.TryGetAsync<List<T>>(listKey);
            if (!cached.Hit)
            {
                return;
            }

            var newList = cached.Value!.ToList();
            foreach (var entity in entityList)
            {
                var idx = newList.FindIndex(x => x.PkEquals(entity));
                if (idx >= 0)
                {
                    newList[idx] = entity;
                }
                else
                {
                    newList.Add(entity);
                }
            }

            await _cache.SetAsync(listKey, newList, CacheTtl);
        }

        private static TimeSpan CacheTtl => Config<CoreConfig>.Get().CacheDefaultTtl;

        private readonly ICacheSession _cache;
        private readonly IDbSession _db;
    }
}

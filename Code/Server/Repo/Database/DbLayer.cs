using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    public class DbLayer : IDbLayer
    {
        private readonly ICacheLayer _cache;
        private readonly IDbExecutorFactory _dbFactory; // DapperExecutorFactory or InMemoryExecutorFactory

        public ICacheLayer Cache => _cache;
        public IDbExecutorFactory DbFactory => _dbFactory;

        public DbLayer(ICacheLayer cache, IDbExecutorFactory dbFactory)
        {
            _cache = cache;
            _dbFactory = dbFactory;
        }

        // ── Select: Cache → DB fallback → Cache Set ───────────
        public T? Get<T>(CacheKey key, Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            var hit = _cache.Get<T>(key);
            if (hit != null)
            {
                return hit;
            }

            var result = _dbFactory.Execute(db => dbFetch(db));

            if (result != null)
            {
                _cache.Set(key, result);
            }

            return result;
        }

        // ── SelectList by PlayerId: Cache → DB → Cache SetList ─
        public List<T> GetListByPlayerId<T>(CacheKey listKey, ulong playerId, Func<T, CacheKey> keySelector) where T : ModelBase
        {
            var cached = _cache.GetList<T>(listKey);
            if (cached != null)
            {
                return cached.ToList();
            }

            var result = _dbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).ToList());
            _cache.BulkSet(result, keySelector);
            return result;
        }

        public List<T> GetListByPlayerIdAndPredicate<T>(CacheKey key, ulong playerId, Func<T, bool> predicate) where T : ModelBase
        {
            var cached = _cache.GetList<T>(key);
            if (cached != null)
            {
                // 캐시값 있으면 쓰고
                return cached.Where(predicate).ToList();
            }

            // DB에서 읽어와도 캐시 갱신은 따로 안함.
            // KeySelector가 들어가야해서 그냥 명시적으로 한번에 해주기 위함인데.. 나중에 마음 바뀌면 개선 검토
            var result = _dbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).ToList());
            return result.Where(predicate).ToList();
        }

        // ── Insert: DB → Cache ─────────────────────────────────
        public T Insert<T>(T entity, CacheKey key) where T : ModelBase
        {
            entity = _dbFactory.Execute(db => db.Insert<T>(entity));
            _cache.Set(key, entity);
            return entity;
        }

        // ── Update: DB → Cache ─────────────────────────────────
        public void Update<T>(T entity, CacheKey key) where T : ModelBase
        {
            _dbFactory.Execute(db => db.Update<T>(entity));
            _cache.Set(key, entity);
        }
    }
}

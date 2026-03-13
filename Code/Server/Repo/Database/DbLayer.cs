using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    public class DbLayer : IDbLayer
    {
        public ICacheLayer Cache { get; }
        public IDbExecutorFactory DbFactory { get; }

        public DbLayer(ICacheLayer cache, IDbExecutorFactory dbFactory)
        {
            Cache = cache;
            DbFactory = dbFactory;
        }

        // ── Select: Cache → DB fallback → Cache Set ───────────
        public T? Get<T>(CacheKey key, Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            var hit = Cache.Get<T>(key);
            if (hit != null)
            {
                return hit;
            }

            var result = DbFactory.Execute(db => dbFetch(db));

            if (result != null)
            {
                Cache.Set(key, result);
            }

            return result;
        }

        // ── SelectList by PlayerId: Cache → DB → Cache SetList ─
        public List<T> GetListByPlayerId<T>(CacheKey listKey, ulong playerId, Func<T, CacheKey> keySelector) where T : ModelBase
        {
            var cached = Cache.GetList<T>(listKey);
            if (cached != null)
            {
                return [.. cached];
            }

            var result = DbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).ToList());
            Cache.BulkSet(result, keySelector);
            return result;
        }

        public List<T> GetListByPlayerIdAndPredicate<T>(CacheKey key, ulong playerId, Func<T, bool> predicate) where T : ModelBase
        {
            var cached = Cache.GetList<T>(key);
            if (cached != null)
            {
                // 캐시값 있으면 쓰고
                return [.. cached.Where(predicate)];
            }

            // DB에서 읽어와도 캐시 갱신은 따로 안함.
            // KeySelector가 들어가야해서 그냥 명시적으로 한번에 해주기 위함인데.. 나중에 마음 바뀌면 개선 검토
            var result = DbFactory.Execute(db => db.SelectListByPlayerId<T>(playerId).ToList());
            return [.. result.Where(predicate)];
        }

        // ── Insert: DB → Cache ─────────────────────────────────
        public T Insert<T>(T entity, CacheKey key) where T : ModelBase
        {
            entity = DbFactory.Execute(db => db.Insert<T>(entity));
            Cache.Set(key, entity);
            return entity;
        }

        // ── Update: DB → Cache ─────────────────────────────────
        public void Update<T>(T entity, CacheKey key) where T : ModelBase
        {
            DbFactory.Execute(db => db.Update<T>(entity));
            Cache.Set(key, entity);
        }
    }
}

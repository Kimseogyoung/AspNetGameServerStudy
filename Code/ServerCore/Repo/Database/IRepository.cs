using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
{
    public interface IRepository
    {
        // 캐시 직접 접근 — 캐시 특화 기능용
        ICacheSession Cache { get; }
        // IDbExecutor 범위 밖 특수 쿼리(SelectListByConditions, 집계 SQL 등) 전용
        IDbSession Db { get; }

        // ── SelectList: Cache → DB fallback → BulkSet ─────────────────────
        List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase;

        // ── Insert: DB Insert → Cache.Set(listKey, entity, match=none) ────
        // listKey: 컬렉션 키. DB Insert 후 auto PK 포함 entity 반환.
        T Insert<T>(T entity, CacheKey listKey) where T : ModelBase;

        // ── Update: DB Update → Cache.Set(listKey, entity, match) ─────────
        void Update<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase;
    }
}

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
        Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase;

        // ── Insert: DB Insert → Cache.Set(listKey, entity, match=none) ────
        // listKey: 컬렉션 키. DB Insert 후 auto PK 포함 entity 반환.
        Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase;

        // ── Insert(캐시 없음): DB Insert 만. 캐시를 지나지 않는다 ────────
        // 리스트 캐시가 없는 엔티티(감사 원장 등)용. 읽기 경로도 없다.
        Task<T> InsertAsync<T>(T entity) where T : ModelBase;

        // ── Update: DB Update → Cache.Set(listKey, entity, match) ─────────
        Task UpdateAsync<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase;

        // ── UpsertList: DB 한 문장 → 캐시 리스트 한 번 갱신 ────────────────
        // 신규/기존을 구분하지 않으므로 GetOrCreate 왕복 없이 여러 행을 저장한다.
        Task UpsertListAsync<T>(IReadOnlyList<T> entityList, CacheKey listKey) where T : ModelBase;
    }
}

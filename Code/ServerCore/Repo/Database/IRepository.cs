using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
{
    // 캐시-어사이드 한 가지만 한다. 캐시를 안 지나는 경로는 GameDb.SessionFor 로 세션을
    // 직접 열지, 여기를 거쳐 가지 않는다 - 그래서 세션도 캐시도 밖으로 내주지 않는다.
    public interface IRepository
    {
        // ── SelectList: Cache → DB fallback → BulkSet ─────────────────────
        Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase;

        // ── Insert: DB Insert → Cache.Set(listKey, entity, match=none) ────
        // listKey: 컬렉션 키. DB Insert 후 auto PK 포함 entity 반환.
        Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase;

        // ── Insert(캐시 없음): DB Insert 만. 캐시를 지나지 않는다 ────────
        // 리스트 캐시가 없는 엔티티(감사 원장 등)용. 읽기 경로도 없다.
        Task<T> InsertAsync<T>(T entity) where T : ModelBase;

        // ── Update: DB Update → Cache.Set(listKey, entity) ────────────────
        // 캐시 리스트에서 바꿀 항목은 PK 로 찾는다. UpsertList 와 같은 기준이다.
        Task UpdateAsync<T>(T entity, CacheKey listKey) where T : ModelBase;

        // ── UpsertList: DB 한 문장 → 캐시 리스트 한 번 갱신 ────────────────
        // 신규/기존을 구분하지 않으므로 GetOrCreate 왕복 없이 여러 행을 저장한다.
        Task UpsertListAsync<T>(IReadOnlyList<T> entityList, CacheKey listKey) where T : ModelBase;
    }
}

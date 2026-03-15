using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    public interface IRepository
    {
        // 캐시 직접 접근 — 캐시 특화 기능용 (R9)
        ICacheSession Cache { get; }
        // IDbExecutor 범위 밖 특수 쿼리(SelectListByConditions, 집계 SQL 등) 전용
        IDbSession Db { get; }

        // Select 단일: Cache → DB fallback → Cache Set (R2, R7)
        T? Get<T>(CacheKey key, Func<IDbExecutor, T?> dbFetch) where T : ModelBase;

        // SelectList: Cache → DB fallback → BulkSet (R2, R4, R10)
        // dbFetch: 호출자가 쿼리 방식을 결정 (PlayerId/AccountId 등 도메인 지식은 호출자 소유)
        List<T> GetList<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, CacheKey> keySelector) where T : ModelBase;

        // SelectList + predicate: 캐시 히트 시 필터 적용, 미스 시 캐시 Set 안함
        List<T> GetListFiltered<T>(CacheKey listKey, Func<IDbExecutor, List<T>> dbFetch, Func<T, bool> predicate) where T : ModelBase;

        // Insert: DB Insert → Cache Set (R2)
        // keyFactory: DB Insert 후 entity(auto-generated PK 포함)로 CacheKey 생성
        T Insert<T>(T entity, Func<T, CacheKey> keyFactory) where T : ModelBase;

        // Update: DB Update → Cache Set (R2)
        void Update<T>(T entity, CacheKey key) where T : ModelBase;
    }
}

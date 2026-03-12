using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo.Database
{
    public interface IDbLayer
    {
        // 캐시 직접 접근 — 캐시 특화 기능용 (R9)
        ICacheLayer Cache { get; }

        // Select 단일: Cache → DB fallback (R2, R7)
        // IDbExecutor로 connection/transaction 숨김 (R11)
        T? Get<T>(CacheKey key, Func<IDbExecutor, T?> dbFetch) where T : ModelBase;

        // SelectList by PlayerId: Cache → DB fallback (R2, R4, R10)
        // 대부분의 리스트 조회는 PlayerId 기준이므로 기본 제공
        // Cache Set함.
        List<T> GetListByPlayerId<T>(CacheKey key, ulong playerId, Func<T, CacheKey> keySelector) where T : ModelBase;

        // predicate: 캐시/DB 결과에 필터 적용. 캐시 Set은 안함.
        List<T> GetListByPlayerIdAndPredicate<T>(CacheKey key, ulong playerId, Func<T, bool> predicate) where T : ModelBase;

        // Insert: DB Insert → Cache Set (R2)
        T Insert<T>(T entity, CacheKey key) where T : ModelBase;

        // Update: DB Update → Cache Set (R2)
        void Update<T>(T entity, CacheKey key) where T : ModelBase;
    }
}

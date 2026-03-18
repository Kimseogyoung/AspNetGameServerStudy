using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    // 컬렉션 단위 캐시 인터페이스.
    // listKey 단위로 List<T> 전체를 저장/조회 — 개별 itemKey 없음.
    // Set은 match predicate로 대상 항목 식별. Invalidate는 컬렉션 전체 제거.
    public interface ICacheSession
    {
        // 반환값: null = 미로드(DB fallback 필요), [] = 빈 컬렉션(로드됨)
        IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase;

        // 컬렉션이 이미 로드된 경우에만 반영. match: 교체 대상 선택, 없으면 추가.
        void Set<T>(CacheKey listKey, T value, Func<T, bool> match, TimeSpan? ttl = null) where T : ModelBase;

        // 컬렉션 전체 적재. keySelector 없음 — List<T> 그대로 저장.
        void BulkSet<T>(CacheKey listKey, IEnumerable<T> values, TimeSpan? ttl = null) where T : ModelBase;

        // 컬렉션 전체 제거 — 다음 GetList 시 DB 재로드.
        void Invalidate(CacheKey listKey);

        // DB 커밋 성공 후 호출 → 지연된 Redis 쓰기 실행
        void FlushPendingWrites();
        // DB 롤백 후 호출 → pending 폐기
        void DiscardPendingWrites();
    }
}

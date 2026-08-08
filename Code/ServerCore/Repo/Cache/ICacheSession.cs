namespace ServerCore.Repo.Cache
{
    // TTL 헬퍼 상수.
    // Set ttl 파라미터:
    //   null            → Config<CoreConfig>.Get().CacheDefaultTtl 자동 적용
    //   CacheTtl.Permanent → TTL 없이 영구 저장
    //   TimeSpan 값     → 지정한 절대 TTL
    public static class CacheTtl
    {
        public static readonly TimeSpan Permanent = Timeout.InfiniteTimeSpan;
    }

    // 조회 결과. async 메서드는 out 파라미터를 못 쓰므로 튜플로 대체.
    public readonly record struct CacheResult<T>(bool Hit, T? Value);

    // 범용 캐시 세션 인터페이스.
    // Get/Set: 자유 자료형 단건 저장/조회 (string 포인터, List<T> 컬렉션 등).
    // Invalidate: 키 제거.
    // FlushPendingWrites: DB 커밋 후 지연된 쓰기(예: Redis) 일괄 반영.
    // DiscardPendingWrites: DB 롤백 후 pending 폐기 (메모리에서 리스트만 비우므로 동기).
    public interface ICacheSession
    {
        Task<CacheResult<T>> TryGetAsync<T>(CacheKey key, TimeSpan? slidingTtl = null);
        Task SetAsync<T>(CacheKey key, T value, TimeSpan? ttl = null);
        Task InvalidateAsync(CacheKey key);
        Task FlushPendingWritesAsync();
        void DiscardPendingWrites();
    }
}

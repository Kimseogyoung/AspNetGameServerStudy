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

    // 범용 캐시 세션 인터페이스.
    // Get/Set: 자유 자료형 단건 저장/조회 (string 포인터, List<T> 컬렉션 등).
    // Invalidate: 키 제거.
    // FlushPendingWrites: DB 커밋 후 지연된 쓰기(예: Redis) 일괄 반영.
    // DiscardPendingWrites: DB 롤백 후 pending 폐기.
    public interface ICacheSession
    {
        bool TryGet<T>(CacheKey key, out T value, TimeSpan? slidingTtl = null);
        void Set<T>(CacheKey key, T value, TimeSpan? ttl = null);
        void Invalidate(CacheKey key);
        void FlushPendingWrites();
        void DiscardPendingWrites();
    }
}

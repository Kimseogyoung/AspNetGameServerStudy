namespace WebStudyServer.Repo.Cache
{
    public interface ICacheLayer
    {
        // ── 읽기 ──────────────────────────────────────────────────────────
        T Get<T>(CacheKey key) where T : class;
        IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : class;

        // ── 쓰기 (ttl: null = 만료 없음, Redis에서만 적용) ─────────────
        void Set<T>(CacheKey key, T value, TimeSpan? ttl = null) where T : class;
        void BulkSet<T>(IEnumerable<T> values, Func<T, CacheKey> keySelector, TimeSpan? ttl = null) where T : class;

        // ── 무효화 ────────────────────────────────────────────────────────
        void Invalidate(CacheKey key);

        // ── 트랜잭션 연동 ─────────────────────────────────────────────────
        // FlushPendingWrites : DB 커밋 성공 후 호출 → 지연된 Redis 쓰기 실행
        void FlushPendingWrites();
        // DiscardPendingWrites: DB 롤백 후 호출 → pending 폐기 + dirty InMemory 키 무효화
        void DiscardPendingWrites();
    }
}

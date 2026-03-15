using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    public interface ICacheSession
    {
        // ── 읽기 ──────────────────────────────────────────────────────────
        T Get<T>(CacheKey key) where T : ModelBase;
        IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase;

        // ── 쓰기 (ttl: null = 만료 없음, Redis에서만 적용) ─────────────
        void Set<T>(CacheKey key, T value, TimeSpan? ttl = null) where T : ModelBase;

        // [BulkSet + GetList prefix 계약]
        // GetList(listKey)는 "listKey.Value를 prefix로 갖는 모든 키"를 반환한다.
        // 따라서 BulkSet의 keySelector는 반드시 아래 조건을 만족해야 한다:
        //   keySelector(item).Value.StartsWith(listKey.Value) == true
        //
        // 올바른 예 (CookieComponent):
        //   listKey = CacheKey.For<CookieModel>(playerId)      → "CookieModel:12345"
        //   itemKey = CacheKey.For<CookieModel>(playerId, num) → "CookieModel:12345:1"  ✅
        //
        // 잘못된 예:
        //   listKey = CacheKey.For<ChannelModel>(accountId)    → "ChannelModel:456"
        //   itemKey = CacheKey.For<ChannelModel>(channelGuid)  → "ChannelModel:abc-guid" ❌
        void BulkSet<T>(IEnumerable<T> values, Func<T, CacheKey> keySelector, TimeSpan? ttl = null) where T : ModelBase;

        // ── 무효화 ────────────────────────────────────────────────────────
        void Invalidate(CacheKey key);

        // ── 트랜잭션 연동 ─────────────────────────────────────────────────
        // FlushPendingWrites : DB 커밋 성공 후 호출 → 지연된 Redis 쓰기 실행
        void FlushPendingWrites();
        // DiscardPendingWrites: DB 롤백 후 호출 → pending 폐기 + dirty InMemory 키 무효화
        void DiscardPendingWrites();
    }
}

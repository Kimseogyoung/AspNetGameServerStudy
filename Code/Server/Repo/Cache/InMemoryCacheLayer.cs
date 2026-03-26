namespace WebStudyServer.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 — 요청 종료 시 인스턴스 자체가 소멸.
    // FlushPendingWrites / DiscardPendingWrites는 no-op.
    public class InMemoryCacheLayer : ICacheSession
    {
        private readonly Dictionary<string, object> _store = [];

        public bool TryGet<T>(CacheKey key, out T outValue, TimeSpan? slidingTtl = null)
        {
            outValue = default;
            if (!_store.TryGetValue(key.Value, out var value))
            {
                return false;
            }

            outValue = (T)value;
            return outValue != null;
        }

        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            _store[key.Value] = value!;
        }

        public void Invalidate(CacheKey key)
        {
            _store.Remove(key.Value);
        }

        public void FlushPendingWrites() { }
        public void DiscardPendingWrites() { }
    }
}

using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 (요청 종료 시 인스턴스 자체가 소멸).
    // GetList: listKey.Value를 prefix로 StartsWith 스캔 — BulkSet으로 저장된 개별 키 대상.
    public class InMemoryCacheLayer : ICacheLayer
    {
        private readonly Dictionary<string, object> _store = [];

        public T Get<T>(CacheKey key) where T : ModelBase
        {
            if (_store.TryGetValue(key.Value, out var value))
            {
                return value as T;
            }

            return null;
        }

        public IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase
        {
            var items = _store
                .Where(kv => kv.Key.StartsWith(listKey.Value))
                .Select(kv => kv.Value as T)
                .Where(v => v != null)
                .ToList();
            return items.Count > 0 ? items : null;
        }

        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null) where T : ModelBase
        {
            _store[key.Value] = value;
        }

        public void BulkSet<T>(IEnumerable<T> values, Func<T, CacheKey> keySelector, TimeSpan? ttl = null) where T : ModelBase
        {
            foreach (var v in values)
            {
                _store[keySelector(v).Value] = v;
            }
        }

        public void Invalidate(CacheKey key)
        {
            _store.Remove(key.Value);
        }

        // Scoped이므로 요청 종료 시 인스턴스 자체가 소멸 → flush/discard 불필요
        public void FlushPendingWrites() { }
        public void DiscardPendingWrites() { }
    }
}

namespace WebStudyServer.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 (요청 종료 시 인스턴스 자체가 소멸).
    // dirty key tracking으로 DB 롤백 시 해당 요청에서 수정된 키만 제거한다.
    public class InMemoryCacheLayer : ICacheLayer
    {
        private readonly Dictionary<string, object> _store = new();
        private readonly HashSet<string> _dirtyKeys = new();

        public T Get<T>(CacheKey key) where T : class
        {
            if (_store.TryGetValue(key.Value, out var value))
            {
                return value as T;
            }
            return null;
        }

        public IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : class
        {
            // TODO: 순환, Linq 개선 필요.
            var keyValues = _store.Where(x => x.Key.StartsWith(listKey.Value)).Select(x=>x.Value).ToList();
            return null;
        }

        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null) where T : class
        {
            _store[key.Value] = value;
            _dirtyKeys.Add(key.Value);
        }

        public void BulkSet<T>(IEnumerable<T> values, Func<T, CacheKey> keySelector, TimeSpan? ttl = null) where T : class
        {
            foreach (var v in values)
            {
                var key = keySelector(v).Value;
                _store[key] = v;
                _dirtyKeys.Add(key);
            }
        }

        public void Invalidate(CacheKey key)
        {
            _store.Remove(key.Value);
        }

        public void FlushPendingWrites()
        {
            // Scoped이므로 요청에서만 _store값이 보존되므로 트랜잭션으로 인해 롤백이나 적용 할필요없음. 즉시 적용함 그냥.
        }

        public void DiscardPendingWrites()
        {
            // Scoped이므로 요청에서만 _store값이 보존되므로 트랜잭션으로 인해 롤백이나 적용 할필요없음. 즉시 적용함 그냥.
        }
    }
}

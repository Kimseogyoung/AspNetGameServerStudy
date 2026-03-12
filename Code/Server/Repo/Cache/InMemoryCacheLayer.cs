namespace WebStudyServer.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 (요청 종료 시 인스턴스 자체가 소멸).
    // dirty key tracking으로 DB 롤백 시 해당 요청에서 수정된 키만 제거한다.
    public class InMemoryCacheLayer : ICacheLayer
    {
        private readonly Dictionary<string, object> _store     = new();
        private readonly HashSet<string>            _dirtyKeys = new();

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
            if (_store.TryGetValue(listKey.Value, out var value))
            {
                return value as List<T>;
            }
            return null;
        }

        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null) where T : class
        {
            _store[key.Value] = value;
            _dirtyKeys.Add(key.Value);
        }

        public void SetList<T>(CacheKey listKey, IEnumerable<T> values, TimeSpan? ttl = null) where T : class
        {
            _store[listKey.Value] = values.ToList();
            _dirtyKeys.Add(listKey.Value);
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

        // DB 커밋 성공 → dirty 목록 초기화 (값은 유지, 이후 조회에서 계속 히트)
        public void FlushPendingWrites()
        {
            _dirtyKeys.Clear();
        }

        // DB 롤백 → 이 요청에서 수정된 키를 모두 제거
        public void DiscardPendingWrites()
        {
            foreach (var key in _dirtyKeys)
            {
                _store.Remove(key);
            }
            _dirtyKeys.Clear();
        }
    }
}

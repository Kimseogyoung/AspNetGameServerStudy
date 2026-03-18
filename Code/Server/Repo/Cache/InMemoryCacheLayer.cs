using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 (요청 종료 시 인스턴스 자체가 소멸).
    //
    // 컬렉션 단위 저장:
    //   _store[listKey] = List<T> — 리스트 전체를 단일 키로 저장.
    //   GetList: _store 직접 조회, null = 미로드.
    //   Set: match predicate로 대상 찾아 교체, 없으면 추가.
    //   BulkSet: _store[listKey] = values — 전체 교체.
    public class InMemoryCacheLayer : ICacheSession
    {
        private readonly Dictionary<string, object> _store = [];

        // null = 미로드, [] = 빈 컬렉션(로드됨)
        public IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase
        {
            return _store.TryGetValue(listKey.Value, out var v) ? (List<T>)v : null;
        }

        public void BulkSet<T>(CacheKey listKey, IEnumerable<T> values, TimeSpan? ttl = null) where T : ModelBase
        {
            _store[listKey.Value] = values.ToList();
        }

        // 컬렉션이 로드된 경우에만 반영 — 부분 적재 방지
        public void Set<T>(CacheKey listKey, T value, Func<T, bool> match, TimeSpan? ttl = null) where T : ModelBase
        {
            if (!_store.TryGetValue(listKey.Value, out var existing))
            {
                return;
            }
            var list = (List<T>)existing;
            var idx = list.FindIndex(x => match(x));
            if (idx >= 0)
            {
                list[idx] = value;
            }
            else
            {
                list.Add(value);
            }
        }

        public void Invalidate(CacheKey listKey)
        {
            _store.Remove(listKey.Value);
        }

        // Scoped이므로 요청 종료 시 인스턴스 자체가 소멸 → flush/discard 불필요
        public void FlushPendingWrites() { }

        public void DiscardPendingWrites() { }
    }
}

using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    // InMemory → Redis 체인 캐시.
    // 읽기: InMemory 히트(로드 완료) → 반환.
    //       미스 → Redis StringGet → InMemory BulkSet 백필 → 반환.
    //       Redis도 미스 → null (DB fallback은 IRepository가 담당).
    // 쓰기: InMemory 즉시 반영 (Read-Your-Writes 보장).
    //       Redis는 FlushPendingWrites() 호출 시 일괄 반영 (DB 커밋 후).
    public class CompositeCacheLayer : ICacheSession
    {
        private readonly InMemoryCacheLayer _memory;
        private readonly RedisCacheLayer _redis;
        private readonly List<Action> _pending = [];

        public CompositeCacheLayer(InMemoryCacheLayer memory, RedisCacheLayer redis)
        {
            _memory = memory;
            _redis = redis;
        }

        // InMemory 미스 시 Redis StringGet → BulkSet으로 InMemory 백필
        public IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase
        {
            var hit = _memory.GetList<T>(listKey);
            if (hit != null)
            {
                return hit;
            }
            var redisHit = _redis.GetList<T>(listKey);
            if (redisHit == null)
            {
                return null;
            }
            _memory.BulkSet(listKey, redisHit);
            return redisHit;
        }

        public void Set<T>(CacheKey listKey, T value, Func<T, bool> match, TimeSpan? ttl = null) where T : ModelBase
        {
            _memory.Set(listKey, value, match, ttl);
            _pending.Add(() => _redis.Set(listKey, value, match, ttl));
        }

        public void BulkSet<T>(CacheKey listKey, IEnumerable<T> values, TimeSpan? ttl = null) where T : ModelBase
        {
            var list = values.ToList();
            _memory.BulkSet(listKey, list, ttl);
            _pending.Add(() => _redis.BulkSet(listKey, list, ttl));
        }

        // Invalidate: 즉시 양쪽 KeyDelete (write-behind 아님)
        public void Invalidate(CacheKey listKey)
        {
            _memory.Invalidate(listKey);
            _redis.Invalidate(listKey);
        }

        // DB 커밋 성공 → pending Redis 쓰기 일괄 실행
        public void FlushPendingWrites()
        {
            foreach (var write in _pending)
            {
                write();
            }
            _pending.Clear();
        }

        // DB 롤백 → pending 폐기 (InMemory는 Scoped이므로 요청 종료 시 소멸)
        public void DiscardPendingWrites()
        {
            _pending.Clear();
        }
    }
}

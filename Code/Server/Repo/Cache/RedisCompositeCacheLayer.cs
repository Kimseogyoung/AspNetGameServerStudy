namespace WebStudyServer.Repo.Cache
{
    // InMemory → Redis 체인 캐시.
    // 읽기: InMemory 히트 → 반환. 미스 → Redis 조회 → InMemory 백필 → 반환.
    //       Redis도 미스 → default (DB fallback은 IRepository가 담당).
    // 쓰기: InMemory 즉시 반영 (Read-Your-Writes 보장).
    //       Redis는 FlushPendingWrites() 호출 시 일괄 반영 (DB 커밋 후).
    public class RedisCompositeCacheLayer : ICacheSession
    {
        private readonly InMemoryCacheLayer _memory;
        private readonly RedisCacheLayer _redis;
        private readonly List<Action> _pending = [];

        public RedisCompositeCacheLayer(InMemoryCacheLayer memory, RedisCacheLayer redis)
        {
            _memory = memory;
            _redis = redis;
        }

        // InMemory 히트 → 반환. 미스 → Redis 조회 → InMemory 백필
        public bool TryGet<T>(CacheKey key, out T outValue)
        {
            if (_memory.TryGet<T>(key, out var memoryHit))
            {
                outValue = memoryHit;
                return true;
            }

            if (_redis.TryGet<T>(key, out var redisHit))
            {
                _memory.Set(key, redisHit);
                outValue = redisHit;
                return true;
            }

            outValue = default(T);
            return false;
        }

        // InMemory 즉시 반영 + Redis pending (DB 커밋 후 flush)
        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            _memory.Set(key, value);
            _pending.Add(() => _redis.Set(key, value, ttl));
        }

        public void Invalidate(CacheKey key)
        {
            _memory.Invalidate(key);
            _redis.Invalidate(key);

            // 애매함... 고민 필요
            _pending.Add(() => _redis.Invalidate(key));
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

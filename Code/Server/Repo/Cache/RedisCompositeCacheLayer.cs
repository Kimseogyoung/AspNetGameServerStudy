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
        // slidingTtl: Redis hit 시 TTL 갱신 (Sliding Expiration)
        public bool TryGet<T>(CacheKey key, out T outValue, TimeSpan? slidingTtl = null)
        {
            if (_memory.TryGet<T>(key, out var memoryHit))
            {
                outValue = memoryHit;
                return true;
            }

            if (_redis.TryGet<T>(key, out var redisHit, slidingTtl))
            {
                _memory.Set(key, redisHit);
                outValue = redisHit;
                return true;
            }

            outValue = default;
            return false;
        }

        // InMemory 즉시 반영 + Redis pending (DB 커밋 후 flush)
        // ttl null → DefaultTtl, CacheTtl.Permanent → 영구. 람다 캡처 전에 resolve.
        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            var resolved = ttl ?? APP.Cfg.CacheDefaultTtl;
            _memory.Set(key, value);
            _pending.Add(() => _redis.Set(key, value, resolved));
        }

        public void Invalidate(CacheKey key)
        {
            _memory.Invalidate(key);
            // 즉시: 롤백 여부와 무관하게 즉시 무효화 (예: Logout)
            _redis.Invalidate(key);
            // pending: 이 Invalidate 이전에 Set이 pending된 경우 flush 시 순서 보장 (Set → Invalidate → 최종 삭제)
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

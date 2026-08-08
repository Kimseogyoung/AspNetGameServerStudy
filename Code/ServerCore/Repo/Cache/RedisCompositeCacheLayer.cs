namespace ServerCore.Repo.Cache
{
    // InMemory → Redis 체인 캐시.
    // 읽기: InMemory 히트 → 반환. 미스 → Redis 조회 → InMemory 백필 → 반환.
    //       Redis도 미스 → default (DB fallback은 IRepository가 담당).
    // 쓰기: InMemory 즉시 반영 (Read-Your-Writes 보장).
    //       Redis는 FlushPendingWrites() 호출 시 일괄 반영 (DB 커밋 후).
    internal class RedisCompositeCacheLayer(InMemoryCacheLayer memory, RedisCacheLayer redis) : ICacheSession
    {
        private readonly List<Func<Task>> _pending = [];

        // InMemory 히트 → 반환. 미스 → Redis 조회 → InMemory 백필
        // slidingTtl: Redis hit 시 TTL 갱신 (Sliding Expiration)
        public async Task<CacheResult<T>> TryGetAsync<T>(CacheKey key, TimeSpan? slidingTtl = null)
        {
            var memoryResult = await memory.TryGetAsync<T>(key);
            if (memoryResult.Hit)
            {
                return memoryResult;
            }

            var redisResult = await redis.TryGetAsync<T>(key, slidingTtl);
            if (redisResult.Hit)
            {
                await memory.SetAsync(key, redisResult.Value);
            }

            return redisResult;
        }

        // InMemory 즉시 반영 + Redis pending (DB 커밋 후 flush)
        // ttl null → DefaultTtl, CacheTtl.Permanent → 영구. 람다 캡처 전에 resolve.
        public async Task SetAsync<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            var resolved = ttl ?? Config<CoreConfig>.Get().CacheDefaultTtl;
            await memory.SetAsync(key, value);
            _pending.Add(() => redis.SetAsync(key, value, resolved));
        }

        public async Task InvalidateAsync(CacheKey key)
        {
            await memory.InvalidateAsync(key);
            // 즉시: 롤백 여부와 무관하게 즉시 무효화 (예: Logout)
            await redis.InvalidateAsync(key);
            // pending: 이 Invalidate 이전에 Set이 pending된 경우 flush 시 순서 보장 (Set → Invalidate → 최종 삭제)
            _pending.Add(() => redis.InvalidateAsync(key));
        }

        // DB 커밋 성공 → pending Redis 쓰기 순서대로 실행
        public async Task FlushPendingWritesAsync()
        {
            foreach (var write in _pending)
            {
                await write();
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

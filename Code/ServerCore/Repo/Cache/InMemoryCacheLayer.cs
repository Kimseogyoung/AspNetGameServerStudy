using Microsoft.Extensions.Logging;

namespace ServerCore.Repo.Cache
{
    // 요청 스코프(Scoped) InMemory 캐시.
    // TTL 미지원 — 요청 종료 시 인스턴스 자체가 소멸.
    // FlushPendingWrites / DiscardPendingWrites는 no-op.
    // I/O가 없어 실제로 블로킹되진 않지만, ICacheSession 시그니처를 맞추기 위해
    // Task.FromResult로 감싼다.
    public class InMemoryCacheLayer(ILogger<InMemoryCacheLayer> logger) : ICacheSession
    {
        private readonly Dictionary<string, object> _store = [];

        public Task<CacheResult<T>> TryGetAsync<T>(CacheKey key, TimeSpan? slidingTtl = null)
        {
            if (!_store.TryGetValue(key.Value, out var value))
            {
                return Task.FromResult(new CacheResult<T>(false, default));
            }

            // 캐스팅 실패는 캐시 오류지 요청 실패 사유가 아님 - 미스로 처리하고 DB fallback에 맡긴다.
            if (value is not T typedValue)
            {
                logger.LogWarning("InMemoryCacheLayer 캐스팅 실패 - 미스로 처리. Key({Key}), ExpectedType({Type})", key.Value, typeof(T).Name);
                return Task.FromResult(new CacheResult<T>(false, default));
            }

            return Task.FromResult(new CacheResult<T>(true, typedValue));
        }

        public Task SetAsync<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            _store[key.Value] = value!;
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CacheKey key)
        {
            _store.Remove(key.Value);
            return Task.CompletedTask;
        }

        public Task FlushPendingWritesAsync() => Task.CompletedTask;
        public void DiscardPendingWrites() { }
    }
}

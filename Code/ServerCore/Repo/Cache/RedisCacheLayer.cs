using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ServerCore.Repo.Cache
{
    // Redis String 기반 캐시 계층 (RedisCompositeCacheLayer 내부 전용이라 internal).
    // Key   = CacheKey.Value
    // Value = string은 raw 저장, 나머지는 JSON 직렬화.
    //
    // Invalidate: KeyDelete — 다음 Get 시 DB 재로드됨.
    // TTL: Key 단위 EXPIRE 설정.
    internal class RedisCacheLayer(IConnectionMultiplexer redis, ILogger<RedisCacheLayer> logger)
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IDatabase _db = redis.GetDatabase();

        // string → raw StringGet/Set, 나머지 → JSON 직렬화
        // slidingTtl: hit 시 TTL 갱신 (Sliding Expiration)
        public async Task<CacheResult<T>> TryGetAsync<T>(CacheKey key, TimeSpan? slidingTtl = null)
        {
            var redisValue = await _db.StringGetAsync(key.Value);
            if (!redisValue.HasValue)
            {
                return new CacheResult<T>(false, default);
            }

            var raw = redisValue.ToString();
            T? outValue;
            try
            {
                outValue = typeof(T) == typeof(string) ? (T)(object)raw : JsonSerializer.Deserialize<T>(raw, JsonOpts);
            }
            catch (Exception e)
            {
                // 저장된 JSON이 T와 안 맞음(모델 리팩터링 등) - 캐시 오류지 요청 실패 사유가 아니므로
                // 미스로 처리하고 DB fallback에 맡긴다. JsonException만이 아니라 커스텀 컨버터가 던질 수
                // 있는 다른 예외까지 포함해서 넓게 잡는다 - fail-open이 목적이라 특정 예외 타입에
                // 의존하면 안 됨.
                logger.LogWarning(e, "CACHE_DESERIALIZE_FAILED - falling back to miss. Key({Key}), ExpectedType({Type})", key.Value, typeof(T).Name);
                return new CacheResult<T>(false, default);
            }

            if (outValue != null && slidingTtl.HasValue)
            {
                await _db.KeyExpireAsync(key.Value, slidingTtl.Value);
            }

            return new CacheResult<T>(outValue != null, outValue);
        }

        public async Task<CacheResult<object>> TryGetAsync(CacheKey key, Type type, TimeSpan? slidingTtl = null)
        {
            var redisValue = await _db.StringGetAsync(key.Value);
            if (!redisValue.HasValue)
            {
                return new CacheResult<object>(false, null);
            }

            var raw = redisValue.ToString();
            object outValue;
            try
            {
                outValue = type == typeof(string) ? raw : JsonSerializer.Deserialize(raw, type, JsonOpts);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "CACHE_DESERIALIZE_FAILED - falling back to miss. Key({Key}), ExpectedType({Type})", key.Value, type.Name);
                return new CacheResult<object>(false, null);
            }

            if (outValue != null && slidingTtl.HasValue)
            {
                await _db.KeyExpireAsync(key.Value, slidingTtl.Value);
            }

            return new CacheResult<object>(outValue != null, outValue);
        }

        public Task SetAsync<T>(CacheKey key, T value, TimeSpan resolvedTtl)
        {
            var raw = typeof(T) == typeof(string)
                ? (string)(object)value!
                : JsonSerializer.Serialize(value, JsonOpts);

            // Permanent → Redis TTL 없음(null).
            var redisTtl = resolvedTtl == CacheTtl.Permanent ? (TimeSpan?)null : resolvedTtl;
            return _db.StringSetAsync(key.Value, raw, redisTtl);
        }

        public Task InvalidateAsync(CacheKey key)
        {
            return _db.KeyDeleteAsync(key.Value);
        }
    }
}

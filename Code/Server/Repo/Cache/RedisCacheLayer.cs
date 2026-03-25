using System.Text.Json;
using StackExchange.Redis;

namespace WebStudyServer.Repo.Cache
{
    // Redis String 기반 캐시 계층 (RedisCompositeCacheLayer 내부 전용).
    // Key   = CacheKey.Value
    // Value = string은 raw 저장, 나머지는 JSON 직렬화.
    //
    // Invalidate: KeyDelete — 다음 Get 시 DB 재로드됨.
    // TTL: Key 단위 EXPIRE 설정.
    public class RedisCacheLayer
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IDatabase _db;

        public RedisCacheLayer(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // string → raw StringGet/Set, 나머지 → JSON 직렬화
        public bool TryGet<T>(CacheKey key, out T outValue)
        {
            outValue = default(T);
            var redisValue = _db.StringGet(key.Value);
            if (!redisValue.HasValue)
            {
                return false;
            }

            var raw = redisValue.ToString();
            if (typeof(T) == typeof(string))
            {
                outValue = (T)(object)raw;
            }
            else
            {
                outValue = JsonSerializer.Deserialize<T>(raw, JsonOpts);
            }

            return outValue != null;
        }

        public void Set<T>(CacheKey key, T value, TimeSpan? ttl = null)
        {
            var raw = typeof(T) == typeof(string)
                ? (string)(object)value!
                : JsonSerializer.Serialize(value, JsonOpts);
            _db.StringSet(key.Value, raw, ttl);
        }

        public void Invalidate(CacheKey key)
        {
            _db.KeyDelete(key.Value);
        }
    }
}

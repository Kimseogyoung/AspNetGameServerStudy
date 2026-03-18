using System.Text.Json;
using StackExchange.Redis;
using WebStudyServer.Model;

namespace WebStudyServer.Repo.Cache
{
    // Redis String 기반 캐시 계층.
    // Key   = listKey  (예: "CookieModel:12345")
    // Value = JSON 배열 전체 (List<T> 직렬화)
    //
    // 컬렉션 단위 저장 — 키 존재 자체가 "완전 적재" 보장.
    // __complete 센티널 불필요.
    //
    // Set: StringGet → 수정 → StringSet (read-modify-write). 캐시 미스(null) 시 skip.
    // Invalidate: KeyDelete — 다음 GetList 시 DB 재로드됨.
    //
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

        // null = 미로드, [] = 빈 컬렉션
        public IReadOnlyList<T> GetList<T>(CacheKey listKey) where T : ModelBase
        {
            var json = (string?)_db.StringGet(listKey.Value);
            return json is null ? null : JsonSerializer.Deserialize<List<T>>(json, JsonOpts);
        }

        public void BulkSet<T>(CacheKey listKey, IEnumerable<T> values, TimeSpan? ttl = null) where T : ModelBase
        {
            var json = JsonSerializer.Serialize(values.ToList(), JsonOpts);
            _db.StringSet(listKey.Value, json, ttl);
        }

        // Read-modify-write: match에 해당하는 항목 교체. 없으면 추가.
        public void Set<T>(CacheKey listKey, T value, Func<T, bool> match, TimeSpan? ttl = null) where T : ModelBase
        {
            var json = (string?)_db.StringGet(listKey.Value);
            if (json is null)
            {
                return;
            }
            var list = JsonSerializer.Deserialize<List<T>>(json, JsonOpts)!;
            var idx = list.FindIndex(x => match(x));
            if (idx >= 0)
            {
                list[idx] = value;
            }
            else
            {
                list.Add(value);
            }
            _db.StringSet(listKey.Value, JsonSerializer.Serialize(list, JsonOpts), ttl);
        }

        public void Invalidate(CacheKey listKey)
        {
            _db.KeyDelete(listKey.Value);
        }
    }
}

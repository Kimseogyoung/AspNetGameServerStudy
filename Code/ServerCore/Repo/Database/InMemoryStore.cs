using System.Collections.Concurrent;

namespace ServerCore.Repo.Database
{
    // InMemory DB의 실제 저장소. Singleton으로 등록해 요청 간 데이터를 유지한다.
    // 타입별로 버킷을 분리하며, PK 키("v1:v2:...")로 엔티티를 관리한다.
    public class InMemoryStore
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> _buckets = new();
        private readonly ConcurrentDictionary<Type, ulong> _autoIncrementCounters = new();

        private ConcurrentDictionary<string, object> Bucket(Type type)
        {
            return _buckets.GetOrAdd(type, _ => new());
        }

        public void Set(Type type, string pkKey, object entity)
        {
            Bucket(type)[pkKey] = entity;
        }

        public IEnumerable<object> GetAll(Type type)
        {
            return Bucket(type).Values;
        }

        public ulong NextAutoId(Type type)
        {
            return _autoIncrementCounters.AddOrUpdate(type, 1UL, (_, prev) => prev + 1UL);
        }
    }
}

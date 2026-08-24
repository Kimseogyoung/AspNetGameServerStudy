using System.Reflection;
using ServerCore.Model;

namespace WebStudyServer.Data
{
    // [Entity]를 런타임에 해석해 둔 것. T마다 한 번만 초기화된다.
    //
    // 값 접근(소유자 읽기/쓰기, PK 비교)은 여기 없다. 생성기가 IScopedModel과 PkEquals로
    // 찍어내므로 문자열을 리플렉션으로 되돌릴 이유가 없다. 여기 남은 것은 SQL에 넣을
    // 컬럼명과 캐시 태그, 즉 코드로 표현할 수 없는 문자열뿐이다.
    public static class EntityMeta<T> where T : ModelBase
    {
        // 소유자 컬럼명. 자동 WHERE에 쓴다. 없으면 null (Auth/Center 계열).
        public static string ScopeKey { get; }

        // 캐시 키 접두사. 캐시를 쓰지 않는 엔티티는 null.
        public static string CacheTag { get; }

        public static bool HasScopeKey => ScopeKey != null;
        public static bool HasCacheTag => CacheTag != null;

        static EntityMeta()
        {
            var type = typeof(T);

            var entity = type.GetCustomAttribute<EntityAttribute>();
            if (entity == null)
            {
                throw new InvalidOperationException($"NOT_FOUND_ENTITY_ATTRIBUTE:{type.Name}");
            }

            if (entity.Pk == null || entity.Pk.Length == 0)
            {
                throw new InvalidOperationException($"EMPTY_ENTITY_PK:{type.Name}");
            }

            ScopeKey = entity.ScopeKey;
            CacheTag = CacheKeyTags.ByModelType.TryGetValue(type, out var tag) ? tag : null;
        }
    }

    public static class EntityMeta
    {
        // 손으로 유지하는 태그 맵의 드리프트를 부팅 시 검사.
        // 정방향은 맵에서 모델을, 역방향은 모델에서 맵을 본다.
        public static void VerifyCacheTags()
        {
            var seen = new Dictionary<string, Type>();

            foreach (var pair in CacheKeyTags.ByModelType)
            {
                var type = pair.Key;
                var tag = pair.Value;

                if (type.GetCustomAttribute<EntityAttribute>() == null)
                {
                    throw new InvalidOperationException($"NOT_ENTITY_CACHE_TAG:{type.Name}");
                }

                if (string.IsNullOrEmpty(tag))
                {
                    throw new InvalidOperationException($"EMPTY_CACHE_TAG:{type.Name}");
                }

                if (seen.TryGetValue(tag, out var owner))
                {
                    throw new InvalidOperationException($"DUPLICATED_CACHE_TAG:{tag}:{owner.Name}:{type.Name}");
                }

                seen[tag] = type;
            }

            // 역방향 — 소유자 축이 있는 엔티티는 캐시 정책을 선언해야 한다.
            // 선언이 없으면 컴파일도 부팅도 되고 첫 Owned<T>() 에서야 터진다.
            // 태그 맵과 모델은 같은 어셈블리에 있다. 인자로 받으면 엉뚱한 어셈블리가
            // 들어와도 조용히 0건으로 통과한다.
            foreach (var type in typeof(CacheKeyTags).Assembly.GetTypes())
            {
                var entity = type.GetCustomAttribute<EntityAttribute>();
                if (entity?.ScopeKey == null)
                {
                    continue;
                }

                var hasTag = CacheKeyTags.ByModelType.ContainsKey(type);
                var noCache = CacheKeyTags.NoCacheByDesign.Contains(type);
                if (hasTag == noCache)
                {
                    throw new InvalidOperationException(hasTag
                        ? $"CACHE_POLICY_CONFLICT:{type.Name}"
                        : $"NOT_DECLARED_CACHE_POLICY:{type.Name}");
                }
            }
        }
    }
}

using System.Linq.Expressions;
using System.Reflection;
using ServerCore.Model;

namespace WebStudyServer.Data
{
    // [Entity] 를 런타임에 해석해 둔 것. 제네릭 static 이므로 T 마다 한 번만
    // 초기화되고, 이후 조회에 딕셔너리 탐색도 리플렉션도 없다.
    //
    // 프로퍼티 접근자는 식 트리로 컴파일해 캐시한다. PropertyInfo.GetValue 를
    // 매번 부르면 소유자 리스트를 읽을 때마다 행 수만큼 리플렉션이 돈다.
    public static class EntityMeta<T> where T : ModelBase
    {
        // 기본 키 컬럼. CSV 행 순서와 같다.
        public static string[] Pk { get; }

        // 소유자 컬럼. 없으면 null (Auth/Center 계열).
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

            Pk = entity.Pk;
            ScopeKey = entity.ScopeKey;
            CacheTag = CacheKeyTags.ByModelType.TryGetValue(type, out var tag) ? tag : null;

            _pkGetters = Pk.Select(CompileGetter).ToArray();
            _scopeKeyGetter = ScopeKey != null ? CompileGetter(ScopeKey) : null;
        }

        // 스코프 키 값. 소유자 리스트의 캐시 키와 자동 WHERE 절에 쓰인다.
        public static object GetScopeKeyValue(T model)
        {
            if (_scopeKeyGetter == null)
            {
                throw new InvalidOperationException($"NOT_FOUND_SCOPE_KEY:{typeof(T).Name}");
            }

            return _scopeKeyGetter(model);
        }

        // IRepository.UpdateAsync 가 요구하는 match 술어. 컴포넌트마다 손으로 쓰던
        // KeyFor 비교를 대신한다.
        //
        // 기존 KeyFor 는 대부분 PK 그대로였고 KingdomStructure 하나만
        // (PlayerId, SfId) 로 스코프 키를 덧붙였는데, match 는 이미 그 플레이어의
        // 리스트 안에서만 쓰이므로 PK 단독과 결과가 같다.
        public static Func<T, bool> PkMatcher(T entity)
        {
            var expected = new object[_pkGetters.Length];
            for (var i = 0; i < _pkGetters.Length; i++)
            {
                expected[i] = _pkGetters[i](entity);
            }

            return x =>
            {
                for (var i = 0; i < _pkGetters.Length; i++)
                {
                    if (!Equals(_pkGetters[i](x), expected[i]))
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        private static Func<T, object> CompileGetter(string propertyName)
        {
            var type = typeof(T);
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new InvalidOperationException($"NOT_FOUND_ENTITY_PROPERTY:{type.Name}.{propertyName}");
            }

            var param = Expression.Parameter(type, "x");
            var body = Expression.Convert(Expression.Property(param, prop), typeof(object));
            return Expression.Lambda<Func<T, object>>(body, param).Compile();
        }

        private static readonly Func<T, object>[] _pkGetters;
        private static readonly Func<T, object> _scopeKeyGetter;
    }

    // 제네릭이 아닌 쪽. 부팅 시 한 번 도는 검사만 갖는다.
    public static class EntityMeta
    {
        // CacheKeyTags.ByModelType 은 손으로 유지하는 목록이라 드리프트가 위험하다.
        // S1 이 없앤 "손으로 쓰는 모델 등록 목록"과 같은 종류의 위험이므로 같은
        // 처방을 쓴다 - 어긋나면 부팅을 실패시킨다.
        //
        // 잡히는 것: 모델 리네임/삭제(엔트리가 [Entity] 없는 타입을 가리키게 됨),
        //            상수 값이 다른 엔티티의 태그와 겹치는 것.
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
        }
    }
}

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace ServerCore.Repo.Database
{
    // IDbExecutor의 InMemory 구현.
    // 성능보다 정확성 우선 — 모든 Select는 전체 스캔(full scan).
    // ASP.NET Core InMemory DB와 동일한 철학:
    //   Insert/Update = 객체 저장, Select = 조건 일치 전체 탐색.
    public class InMemoryDbExecutor : IDbExecutor
    {
        private readonly InMemoryStore _store;

        // 조건 타입 → PropertyInfo[] 캐시 (ScanFirst / ScanAll 공용)
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CondPropsCache = new();

        // (엔티티 타입, 프로퍼티명) → PropertyInfo 캐시
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> EntityPropCache = new();

        // 타입 → Id PropertyInfo 캐시 (Insert 자동증가 판단용)
        private static readonly ConcurrentDictionary<Type, PropertyInfo> IdPropCache = new();

        public InMemoryDbExecutor(InMemoryStore store)
        {
            _store = store;
        }

        public Task<T> SelectByPkAsync<T>(object param) where T : class
        {
            return Task.FromResult(ScanFirst<T>(param));
        }

        public Task<T> SelectByConditionsAsync<T>(object conditions) where T : class
        {
            return Task.FromResult(ScanFirst<T>(conditions));
        }

        public Task<IEnumerable<T>> SelectListByConditionsAsync<T>(object conditions) where T : class
        {
            if (conditions == null)
            {
                return Task.FromResult(_store.GetAll(typeof(T)).Cast<T>());
            }

            return Task.FromResult(ScanAll<T>(conditions));
        }

        public Task<IEnumerable<T>> SelectListByColumnAsync<T>(string column, object value) where T : class
        {
            var prop = EntityPropCache.GetOrAdd((typeof(T), column), k => k.Item1.GetProperty(k.Item2));
            if (prop == null)
            {
                throw new InvalidOperationException($"NOT_FOUND_ENTITY_PROPERTY:{typeof(T).Name}.{column}");
            }

            var matched = _store.GetAll(typeof(T)).Cast<T>()
                .Where(e => ValuesEqual(value, prop.GetValue(e), prop.PropertyType));
            return Task.FromResult(matched);
        }

        public Task<T> InsertAsync<T>(T entity) where T : class
        {
            // SQL의 AUTO_INCREMENT 동작 모방:
            // Id 프로퍼티가 있고 값이 0이면 스토어에서 다음 ID를 발급한다.
            // Id를 호출부가 직접 지정한 경우(예: PlayerModel)는 0이 아니므로 건너뜀.
            var type = typeof(T);
            var idProp = IdPropCache.GetOrAdd(type, t => t.GetProperty("Id"));
            if (idProp != null && Convert.ToUInt64(idProp.GetValue(entity)) == 0UL)
            {
                idProp.SetValue(entity, _store.NextAutoId(type));
            }

            _store.Set(type, InMemoryPkRegistry.ComputePkKey(entity), entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync<T>(T entity) where T : class
        {
            _store.Set(typeof(T), InMemoryPkRegistry.ComputePkKey(entity), entity);
            return Task.CompletedTask;
        }

        // InMemory 모드에서는 집계 SQL을 실행할 수 없으므로 NotSupportedException.
        public Task<T> QuerySingleAsync<T>(string sql, object param)
        {
            throw new NotSupportedException();
        }

        private T ScanFirst<T>(object conditions) where T : class
        {
            if (conditions == null)
            {
                return null;
            }

            var condProps = CondPropsCache.GetOrAdd(
                conditions.GetType(),
                t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            return _store.GetAll(typeof(T)).Cast<T>().FirstOrDefault(e => MatchAll(e, conditions, condProps));
        }

        private IEnumerable<T> ScanAll<T>(object conditions) where T : class
        {
            var condProps = CondPropsCache.GetOrAdd(
                conditions.GetType(),
                t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            return _store.GetAll(typeof(T)).Cast<T>().Where(e => MatchAll(e, conditions, condProps));
        }

        // conditions의 모든 non-null 프로퍼티가 entity와 일치하면 true
        private bool MatchAll(object entity, object conditions, PropertyInfo[] condProps)
        {
            var entityType = entity.GetType();
            foreach (var condProp in condProps)
            {
                var condVal = condProp.GetValue(conditions);
                if (condVal == null)
                {
                    continue;
                }

                var entityProp = EntityPropCache.GetOrAdd(
                    (entityType, condProp.Name),
                    k => k.Item1.GetProperty(k.Item2));
                if (entityProp == null)
                {
                    return false;
                }

                var entityVal = entityProp.GetValue(entity);

                // IList 조건이면 Contains, 스칼라면 값 비교
                if (condVal is IList list)
                {
                    if (!list.Cast<object>().Any(v => ValuesEqual(v, entityVal, entityProp.PropertyType)))
                    {
                        return false;
                    }
                }
                else
                {
                    // 타입이 다를 수 있으므로(예: int vs ulong, enum vs int) 엔티티 프로퍼티 타입으로 변환 후 비교
                    if (!ValuesEqual(condVal, entityVal, entityProp.PropertyType))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValuesEqual(object condVal, object entityVal, Type targetType)
        {
            try
            {
                var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
                var converted = Convert.ChangeType(condVal, underlying);
                return Equals(converted, entityVal);
            }
            catch
            {
                return Equals(condVal, entityVal);
            }
        }

    }
}

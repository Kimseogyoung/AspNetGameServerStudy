using System.Reflection;

namespace WebStudyServer.Repo.Database
{
    // IDbExecutor의 InMemory 구현.
    // 성능보다 정확성 우선 — 모든 Select는 전체 스캔(full scan).
    // ASP.NET Core InMemory DB와 동일한 철학:
    //   Insert/Update = 객체 저장, Select = 조건 일치 전체 탐색.
    public class InMemoryDbExecutor : IDbExecutor
    {
        private readonly InMemoryStore _store;

        public InMemoryDbExecutor(InMemoryStore store)
        {
            _store = store;
        }

        public T SelectByPk<T>(object param) where T : class
        {
            return ScanFirst<T>(param);
        }

        public T SelectByConditions<T>(object conditions) where T : class
        {
            return ScanFirst<T>(conditions);
        }

        public IEnumerable<T> SelectListByConditions<T>(object conditions) where T : class
        {
            if (conditions == null)
            {
                return _store.GetAll(typeof(T)).Cast<T>();
            }

            return ScanAll<T>(conditions);
        }

        public T Insert<T>(T entity) where T : class
        {
            // SQL의 AUTO_INCREMENT 동작 모방:
            // Id 프로퍼티가 있고 값이 0이면 스토어에서 다음 ID를 발급한다.
            // Id를 호출부가 직접 지정한 경우(예: PlayerModel)는 0이 아니므로 건너뜀.
            var type = typeof(T);
            var idProp = type.GetProperty("Id");
            if (idProp != null && Convert.ToUInt64(idProp.GetValue(entity)) == 0UL)
            {
                idProp.SetValue(entity, _store.NextAutoId(type));
            }

            _store.Set(type, InMemoryPkRegistry.ComputePkKey(entity), entity);
            return entity;
        }

        public void Update<T>(T entity) where T : class
        {
            _store.Set(typeof(T), InMemoryPkRegistry.ComputePkKey(entity), entity);
        }

        // InMemory 모드에서는 집계 SQL을 실행할 수 없으므로 NotSupportedException.
        public T QuerySingle<T>(string sql, object param)
        {
            throw new NotSupportedException();
        }

        private T ScanFirst<T>(object conditions) where T : class
        {
            if (conditions == null)
            {
                return null;
            }

            var condType = conditions.GetType();
            return _store.GetAll(typeof(T)).Cast<T>().FirstOrDefault(e => MatchAll(e, conditions, condType));
        }

        private IEnumerable<T> ScanAll<T>(object conditions) where T : class
        {
            var condType = conditions.GetType();
            return _store.GetAll(typeof(T)).Cast<T>().Where(e => MatchAll(e, conditions, condType));
        }

        // conditions의 모든 non-null 프로퍼티가 entity와 일치하면 true
        private static bool MatchAll(object entity, object conditions, Type condType)
        {
            var entityType = entity.GetType();
            foreach (var prop in condType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var condVal = prop.GetValue(conditions);
                if (condVal == null)
                {
                    continue;
                }

                var entityProp = entityType.GetProperty(prop.Name);
                if (entityProp == null)
                {
                    return false;
                }

                var entityVal = entityProp.GetValue(entity);
                // 타입이 다를 수 있으므로(예: int vs ulong, enum vs int) 엔티티 프로퍼티 타입으로 변환 후 비교
                if (!ValuesEqual(condVal, entityVal, entityProp.PropertyType))
                {
                    return false;
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

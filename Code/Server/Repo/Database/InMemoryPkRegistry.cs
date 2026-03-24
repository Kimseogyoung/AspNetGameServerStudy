using System.Collections.Concurrent;
using System.Reflection;

namespace WebStudyServer.Repo.Database
{
    // InMemory 전용 PK 필드 레지스트리.
    // DapperExtension.Init 옆에서 함께 호출해 등록한다.
    // DapperExtension은 변경하지 않는다.
    public static class InMemoryPkRegistry
    {
        private static readonly ConcurrentDictionary<Type, string[]> KeyFields = new();

        // PK 필드명 → PropertyInfo 캐시 (ComputePkKey에서 GetProperty 반복 호출 제거)
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PkProps = new();

        public static void Init<T>(params string[] keyFields)
        {
            var type = typeof(T);
            KeyFields[type] = keyFields;
            PkProps[type] = keyFields.Select(f => type.GetProperty(f)).ToArray();
        }

        public static string[] GetKeyFields(Type type)
        {
            if (!KeyFields.TryGetValue(type, out var fields))
            {
                throw new InvalidOperationException(
                    $"InMemoryPkRegistry: {type.Name} 미등록. Init<T>()를 먼저 호출하세요.");
            }

            return fields;
        }

        // 엔티티에서 PK 값을 "v1:v2:..." 형식으로 조합
        public static string ComputePkKey(object entity)
        {
            var type = entity.GetType();
            var props = PkProps[type];
            return string.Join(":", props.Select(p => p?.GetValue(entity)?.ToString() ?? "null"));
        }
    }
}

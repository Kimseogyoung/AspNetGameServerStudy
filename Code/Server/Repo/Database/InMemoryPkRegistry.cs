using System.Collections.Concurrent;

namespace WebStudyServer.Repo.Database
{
    // InMemory 전용 PK 필드 레지스트리.
    // DapperExtension.Init 옆에서 함께 호출해 등록한다.
    // DapperExtension은 변경하지 않는다.
    public static class InMemoryPkRegistry
    {
        private static readonly ConcurrentDictionary<Type, string[]> KeyFields = new();

        public static void Init<T>(params string[] keyFields)
        {
            KeyFields[typeof(T)] = keyFields;
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
            var fields = GetKeyFields(type);
            return string.Join(":", fields.Select(f =>
                type.GetProperty(f)?.GetValue(entity)?.ToString() ?? "null"));
        }
    }
}

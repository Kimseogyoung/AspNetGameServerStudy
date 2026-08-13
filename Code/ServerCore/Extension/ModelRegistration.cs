using System.Collections.Concurrent;
using ServerCore.Repo.Database;

namespace ServerCore.Extension
{
    // Init<T> 한 줄로 DapperExtension + InMemoryPkRegistry 동시 등록
    public static class ModelRegistration
    {
        // 어떤 PK 로 등록했는지 기억한다. 아래 충돌 검사에만 쓴다.
        private static readonly ConcurrentDictionary<Type, string[]> _registeredDict = new();

        public static void Init<T>(params string[] keyFields)
        {
            var type = typeof(T);

            // 두 레지스트리 모두 Dictionary[type] = 값 형태의 덮어쓰기라, 같은 모델이
            // 서로 다른 PK 로 두 번 등록되면 나중 것이 조용히 이긴다. 그 PK 는 그대로
            // WHERE 절과 InMemory 키에 박히고, 증상은 예외 없이 "0행 매치"나
            // "캐시 미스"로 나타나 추적이 어렵다. 부팅 시점에 터뜨린다.
            //
            // 같은 값의 재등록은 허용한다 — ServerTest 가 WebApplicationFactory 를
            // 여러 번 만들면 Init 이 반복 호출된다.
            if (_registeredDict.TryGetValue(type, out var prevKeyFields) && !prevKeyFields.SequenceEqual(keyFields))
            {
                throw new InvalidOperationException($"PK_REGISTRATION_CONFLICT:{type.Name} [{string.Join(", ", prevKeyFields)}] vs [{string.Join(", ", keyFields)}]");
            }

            DapperExtension.Init<T>(keyFields);
            InMemoryPkRegistry.Init<T>(keyFields);
            _registeredDict[type] = keyFields;
        }
    }
}

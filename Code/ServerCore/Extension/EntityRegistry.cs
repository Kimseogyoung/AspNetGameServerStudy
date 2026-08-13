using System;
using System.Reflection;
using ServerCore.Model;

namespace ServerCore.Extension
{
    // [Entity] 가 붙은 모델을 어셈블리에서 찾아 ModelRegistration 에 등록한다.
    //
    // 손으로 쓰는 등록 목록을 대체하기 위한 것이다. 목록이 손으로 관리되는 동안은
    // Server 와 RaidServer 가 서로 다른 부분집합을 등록할 수 있고(실제로 그랬다),
    // 어긋난 사실은 해당 요청이 실패할 때까지 드러나지 않는다. 스캔으로 등록하면
    // 두 호스트가 같은 목록을 갖는 것이 구조적으로 보장된다.
    public static class EntityRegistry
    {
        public static void ScanAndRegister(Assembly assembly)
        {
            // ModelRegistration.Init<T> 는 제네릭이라 타입별로 닫아서 호출해야 한다.
            var initMethod = typeof(ModelRegistration).GetMethod(nameof(ModelRegistration.Init));
            if (initMethod == null)
            {
                throw new InvalidOperationException("NOT_FOUND_METHOD:ModelRegistration.Init");
            }

            foreach (var type in assembly.GetTypes())
            {
                var entity = type.GetCustomAttribute<EntityAttribute>();
                if (entity == null)
                {
                    continue;
                }

                if (entity.Pk == null || entity.Pk.Length == 0)
                {
                    throw new InvalidOperationException($"EMPTY_ENTITY_PK:{type.Name}");
                }

                try
                {
                    // Init<T>(params string[]) 이므로 배열 하나를 인자 하나로 넘긴다.
                    initMethod.MakeGenericMethod(type).Invoke(null, new object[] { entity.Pk });
                }
                catch (TargetInvocationException e) when (e.InnerException != null)
                {
                    // 리플렉션 래핑을 벗겨 PK_REGISTRATION_CONFLICT 가 그대로 보이게 한다.
                    throw e.InnerException;
                }
            }
        }
    }
}

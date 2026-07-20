using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServerCore
{
    public interface IConfig
    {
        void Init(IConfiguration config, IHostEnvironment environ);
    }

    // IConfig를 구현한 모든 타입을 리플렉션으로 찾아 각각 Config<T>.Init을 호출한다.
    // 새 Config 클래스가 추가돼도 여기 손댈 필요 없이 자동으로 로드됨.
    public static class Config
    {
        public static void InitAll(IConfiguration config, IHostEnvironment environ)
        {
            foreach (var type in FindConfigTypes())
            {
                var configType = typeof(Config<>).MakeGenericType(type);
                var initMethod = configType.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                initMethod!.Invoke(null, [config, environ]);
            }
        }

        private static IEnumerable<Type> FindConfigTypes()
        {
            LoadReferencedAssemblies();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(IConfig).IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) != null);
        }

        // AppDomain.CurrentDomain.GetAssemblies()는 "이미 로드된" 어셈블리만 보여준다.
        // IConfig 구현체가 있는 어셈블리(예: DbModel)가 이 시점까지 JIT에 의해 로드되지
        // 않았을 수 있으므로, 현재 로드된 어셈블리들의 참조를 재귀적으로 강제 로드한다.
        private static void LoadReferencedAssemblies()
        {
            var loadedNames = new HashSet<string>(
                AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name));
            var toVisit = new Queue<Assembly>(AppDomain.CurrentDomain.GetAssemblies());

            while (toVisit.Count > 0)
            {
                var assembly = toVisit.Dequeue();
                foreach (var refName in assembly.GetReferencedAssemblies())
                {
                    if (!loadedNames.Add(refName.Name))
                    {
                        continue;
                    }

                    try
                    {
                        toVisit.Enqueue(Assembly.Load(refName));
                    }
                    catch
                    {
                        // 플랫폼 전용 등 로드 불가한 참조는 건너뜀
                    }
                }
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }

    // 프로세스 전역 self-singleton 레지스트리. IConfig를 구현하는 Config 클래스라면
    // 동일한 방법(Config<T>.Init / Config<T>.Get)으로 접근 가능.
    // RaidServer(별도 프로세스/DI 컨테이너), DbModel의 non-DI Manager/Component 계층처럼
    // DI가 닿지 않는 곳에서도 참조해야 해서 static으로 유지.
    public static class Config<T> where T : class, IConfig, new()
    {
        private static T _instance;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_instance != null)
            {
                return; // 이미 로드됨 — 재호출은 무시 (기존 _isInit 가드와 동일)
            }

            var instance = new T();
            instance.Init(config, environ);
            _instance = instance;
        }

        public static T Get() =>
            _instance ?? throw new InvalidOperationException($"{typeof(T).Name} not initialized. Call Config<{typeof(T).Name}>.Init first.");
    }
}

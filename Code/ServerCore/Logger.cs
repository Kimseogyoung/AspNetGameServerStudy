using Microsoft.Extensions.Logging;

namespace ServerCore
{
    // 프로세스 전역 self-singleton. RaidServer(별도 프로세스/DI 컨테이너), DbModel의 non-DI
    // Manager/Component 계층처럼 DI가 닿지 않는 곳에서도 참조해야 해서 static으로 유지.
    // DI가 구성한 것과 동일한 ILoggerFactory를 그대로 넘겨받아 쓰므로, Startup.Logging()에서
    // 설정한 provider/필터(Server는 NLog, RaidServer는 Console)와 항상 동일하게 동작한다.
    public static class Logger
    {
        private static ILogger _instance;

        public static void Init(ILoggerFactory factory)
        {
            if (_instance != null)
            {
                return;
            }

            _instance = factory.CreateLogger("Main");
        }

        public static ILogger Get() =>
            _instance ?? throw new InvalidOperationException("Logger not initialized. Call Logger.Init first.");
    }
}

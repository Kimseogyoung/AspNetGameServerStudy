using Microsoft.Extensions.Logging;

namespace ServerCore
{
    // 프로세스 전역 self-singleton. RaidServer(별도 프로세스/DI 컨테이너), DbModel의 non-DI
    // Manager/Component 계층처럼 DI가 닿지 않는 곳에서도 참조해야 해서 static으로 유지.
    public static class LoggerProvider
    {
        private static ILogger _instance;

        public static void Init(CoreConfig cfg)
        {
            if (_instance != null)
            {
                return;
            }

            _instance = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(cfg.LogLevel);
            }).CreateLogger("Core");
        }

        public static ILogger Get() =>
            _instance ?? throw new InvalidOperationException("LoggerProvider not initialized. Call LoggerProvider.Init first.");

        public static void LogError(Exception exception, string message, params object[] args) =>
            Get().LogError(exception, message, args);
    }
}

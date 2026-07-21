using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ServerCore
{
    public static class Core
    {
        public static ILogger Logger { get; private set; } = NullLogger.Instance;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            Config<CoreConfig>.Init(config, environ);
            var cfg = Config<CoreConfig>.Get();

            IdGeneratorProvider.Init(cfg);

            Logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(cfg.LogLevel);
            }).CreateLogger("Core");
        }

        private static bool _isInit = false;
    }
}

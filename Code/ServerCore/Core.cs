using IdGen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ServerCore
{
    public static class Core
    {
        public static IdGenerator IdGenerator { get; private set; } = null!;
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

            var workerId = cfg.ServerNum == -1 ? new Random().Next(1024) : cfg.ServerNum;
            IdGenerator = new IdGenerator(workerId);

            Logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(cfg.LogLevel);
            }).CreateLogger("Core");
        }

        private static bool _isInit = false;
    }
}

using IdGen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ServerCore
{
    public static class Core
    {
        public static CoreConfig Cfg { get; } = new CoreConfig();
        public static IdGenerator IdGenerator { get; private set; } = null!;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            Cfg.Init(config, environ);

            var workerId = Cfg.ServerNum == -1 ? new Random().Next(1024) : Cfg.ServerNum;
            IdGenerator = new IdGenerator(workerId);
        }

        private static bool _isInit = false;
    }
}

using IdGen;
using Proto;

namespace WebStudyServer.GAME
{
    public static class APP
    {
        public static ConfigSystem Cfg { get; } = new ConfigSystem();
        public static ProtoSystem Prt { get; } = new ProtoSystem();
        public static IdGenerator IdGenerator { get; private set; } = null;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_isInit)
            {
                // TODO: 로그
                return;
            }

            _isInit = true;
            Cfg.Init(config, environ);
            Prt.Init(config, environ);

            var workerId = Cfg.ServerNum == -1 ? new Random().Next(1024) : Cfg.ServerNum;
            IdGenerator = new IdGenerator(workerId);
        }

        private static bool _isInit = false;
    }
}

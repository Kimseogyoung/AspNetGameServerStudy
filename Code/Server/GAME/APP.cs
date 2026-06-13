using IdGen;
using Proto;
using ServerCore;

namespace WebStudyServer.GAME
{
    public static class APP
    {
        public static GameConfig Cfg { get; } = new GameConfig();
        public static ProtoSystem Prt { get; } = new ProtoSystem();
        public static IdGenerator IdGenerator => Core.IdGenerator;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_isInit)
            {
                // TODO: 로그
                return;
            }

            _isInit = true;
            Core.Init(config, environ);
            Cfg.Init(config, environ);
            Prt.Init(config, environ);
        }

        private static bool _isInit = false;
    }
}

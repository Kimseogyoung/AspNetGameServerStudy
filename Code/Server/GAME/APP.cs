using IdGen;
using Proto;

namespace WebStudyServer.GAME
{
    public static class APP
    {
        public static ConfigSystem Cfg => _cfg;
        public static ProtoSystem Prt => _prt;
        public static IdGenerator IdGenerator => _idGen;

        public static void Init(IConfiguration config, IHostEnvironment environ)
        {
            if (_isInit)
            {
                // TODO: 로그
                return;
            }

            _isInit = true;
            _cfg.Init(config, environ);
            _prt.Init(config, environ);

            var workerId = Cfg.ServerNum == -1 ? new Random().Next(1024) : Cfg.ServerNum;
            _idGen = new IdGenerator(workerId);
        }

        private static bool _isInit = false;
        private static IdGenerator _idGen = null;
        private static ConfigSystem _cfg = new ConfigSystem();
        private static ProtoSystem _prt = new ProtoSystem();
    }
}

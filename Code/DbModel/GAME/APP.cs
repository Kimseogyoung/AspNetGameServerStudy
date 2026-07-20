using IdGen;
using ServerCore;

namespace WebStudyServer.GAME
{
    public static class APP
    {
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
            Config.InitAll(config, environ); // GameConfig 등 IConfig 구현체를 리플렉션으로 전부 로드
        }

        private static bool _isInit = false;
    }
}

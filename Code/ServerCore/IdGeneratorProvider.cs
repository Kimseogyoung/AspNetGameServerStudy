using IdGen;

namespace ServerCore
{
    // 프로세스 전역 self-singleton. RaidServer(별도 프로세스/DI 컨테이너), DbModel의 non-DI
    // Manager/Component 계층처럼 DI가 닿지 않는 곳에서도 참조해야 해서 static으로 유지.
    public static class IdGeneratorProvider
    {
        private static IdGenerator _instance;

        public static void Init(CoreConfig cfg)
        {
            if (_instance != null)
            {
                return;
            }

            var workerId = cfg.ServerNum == -1 ? new Random().Next(1024) : cfg.ServerNum;
            _instance = new IdGenerator(workerId);
        }

        public static IdGenerator Get() => _instance ?? throw new InvalidOperationException("IdGeneratorProvider not initialized. Call IdGeneratorProvider.Init first.");
    }
}

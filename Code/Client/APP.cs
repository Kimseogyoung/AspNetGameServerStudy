using IdGen;

namespace Client
{
    public static class APP
    {
        public static ProtoSystem Prt => _prt;
        public static ContextSystem Ctx => _ctx;

        public static void Init(string inCsvPath)
        {
            if (_isInit)
            {
                // TODO: 로그
                return;
            }

            var csvPath = Path.Join(GetProjPath(), inCsvPath);
            Console.WriteLine(csvPath);

            _isInit = true;
            _prt.Init(csvPath);
            _ctx.Init();
        }

        private static string GetProjPath()
        {
            var exeCfgDirNetPath = Path.GetDirectoryName(AppContext.BaseDirectory);

            var exeCfgDirPath = Path.GetDirectoryName(exeCfgDirNetPath);
            var binDirPath = Path.GetDirectoryName(exeCfgDirPath);
            var projectPath = Path.GetDirectoryName(binDirPath);

            if (AppContext.BaseDirectory.Contains("Dist"))
            {
                projectPath = exeCfgDirNetPath;
            }

            return projectPath == null ? string.Empty : projectPath;
        }

        private static bool _isInit = false;
        private static ProtoSystem _prt = new ProtoSystem();
        private static ContextSystem _ctx = new ContextSystem();
    }


}

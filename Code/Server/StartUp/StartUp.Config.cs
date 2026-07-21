using ServerCore;
using WebFramework.Config;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Config(WebApplicationBuilder builder, string workPath = "")
        {
            workPath = string.IsNullOrEmpty(workPath) ? Directory.GetCurrentDirectory() : workPath;

            builder.Configuration
             .SetBasePath(workPath)
             .AddYamlFile("appsettings.yaml", optional: false)
             .AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", optional: true)
             .AddEnvironmentVariables();

            Config<CoreConfig>.Init(builder.Configuration, builder.Environment);
            var cfg = Config<CoreConfig>.Get();
            IdGeneratorProvider.Init(cfg);
            ServerCore.Config.InitAll(builder.Configuration, builder.Environment); // GameConfig 등 IConfig 구현체를 리플렉션으로 전부 로드
        }
    }
}

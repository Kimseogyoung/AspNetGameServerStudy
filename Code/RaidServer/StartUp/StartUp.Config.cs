using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ServerCore;
using WebFramework.Config;

namespace RaidServer
{
    public partial class Startup
    {
        public void Config(HostApplicationBuilder builder, string workPath = "")
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
            LoggerProvider.Init(cfg);
        }
    }
}

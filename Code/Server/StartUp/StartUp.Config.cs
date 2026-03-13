using WebFramework.Config;
using WebStudyServer.GAME;

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

            APP.Init(builder.Configuration, builder.Environment);
        }
    }
}

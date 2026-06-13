using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerCore;

namespace RaidServer
{
    public partial class Startup
    {
        public void Logging(HostApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(Core.Cfg.LogLevel);
        }
    }
}

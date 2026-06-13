using Microsoft.Extensions.Configuration;

namespace RaidServer
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}

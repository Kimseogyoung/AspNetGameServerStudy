using Microsoft.Extensions.Configuration;

namespace RaidServer
{
    public class RaidConfig
    {
        public int Port { get; }

        public RaidConfig(IConfiguration config)
        {
            Port = config.GetValue("Raid:Port", 5000);
        }
    }
}

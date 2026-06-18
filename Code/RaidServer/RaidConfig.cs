using Microsoft.Extensions.Configuration;

namespace RaidServer
{
    public class RaidConfig
    {
        public int Port { get; }
        public int PingIntervalSec { get; }
        public int SessionTimeoutSec { get; }
        public int MatchingTickIntervalSec { get; }
        public int MatchingTimeoutSec { get; }

        public RaidConfig(IConfiguration config)
        {
            Port = config.GetValue("Raid:Port", 5000);
            PingIntervalSec = config.GetValue("Raid:PingIntervalSec", 10);
            SessionTimeoutSec = config.GetValue("Raid:SessionTimeoutSec", 30);
            MatchingTickIntervalSec = config.GetValue("Raid:MatchingTickIntervalSec", 2);
            MatchingTimeoutSec = config.GetValue("Raid:MatchingTimeoutSec", 30);
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaidServer.Network;

namespace RaidServer
{
    public class SocketClientListener : BackgroundService
    {
        public SocketClientListener(SocketService socketService, PacketProcessor packetProcessor, RaidConfig raidConfig, ILogger<SocketClientListener> logger)
        {
            _packetProcessor = packetProcessor;
            _socketService = socketService;
            _raidConfig = raidConfig;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _socketService.StartAsync(_raidConfig.Port, stoppingToken, _packetProcessor.AddPacket);
        }

        private readonly PacketProcessor _packetProcessor;
        private readonly SocketService _socketService;
        private readonly RaidConfig _raidConfig;
        private readonly ILogger<SocketClientListener> _logger;
    }
}

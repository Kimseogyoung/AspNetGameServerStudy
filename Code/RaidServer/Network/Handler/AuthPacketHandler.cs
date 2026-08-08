using Microsoft.Extensions.Logging;
using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class AuthPacketHandler : PacketHandlerBase<AuthRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.AuthRequest;

        public AuthPacketHandler(SessionService sessionService, PlayerRaidSessionService playerRaidSessionService, RaidConfig raidConfig, ILogger<AuthPacketHandler> logger)
        {
            _sessionService = sessionService;
            _playerRaidSessionService = playerRaidSessionService;
            _raidConfig = raidConfig;
            _logger = logger;
        }

        protected override async Task RunAsync(string sessionId, AuthRequestPacket req)
        {
            var res = await _playerRaidSessionService.AuthenticateAsync(sessionId, req);

            if (res.Result == EAuthResult.Success)
            {
                res.PingIntervalSec = _raidConfig.PingIntervalSec;
                _logger.LogInformation($"AuthSuccess SessionId({sessionId}) AccountId({res.AccountId}) PlayerId({res.PlayerId}) ShardId({res.ShardId})");
            }
            else
            {
                _logger.LogWarning($"AuthFailed SessionId({sessionId}) Result({res.Result})");
            }

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.AuthResponse,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });
        }

        private readonly SessionService _sessionService;
        private readonly PlayerRaidSessionService _playerRaidSessionService;
        private readonly RaidConfig _raidConfig;
        private readonly ILogger<AuthPacketHandler> _logger;
    }
}

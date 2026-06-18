using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class AuthPacketHandler : PacketHandlerBase<AuthRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.AuthRequest;

        public AuthPacketHandler(SessionService sessionService, PlayerRaidSessionService playerRaidSessionService)
        {
            _sessionService = sessionService;
            _playerRaidSessionService = playerRaidSessionService;
        }

        protected override Task RunAsync(string sessionId, AuthRequestPacket req)
        {
            var res = _playerRaidSessionService.Authenticate(sessionId, req);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.AuthResponse,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
        private readonly PlayerRaidSessionService _playerRaidSessionService;
    }
}

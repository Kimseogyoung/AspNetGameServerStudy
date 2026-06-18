using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class AuthPacketHandler : PacketHandlerBase<AuthReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.AuthReq;

        public AuthPacketHandler(SessionService sessionService, PlayerRaidSessionService playerRaidSessionService)
        {
            _sessionService = sessionService;
            _playerRaidSessionService = playerRaidSessionService;
        }

        protected override Task RunAsync(string sessionId, AuthReqPacket req)
        {
            var res = _playerRaidSessionService.Authenticate(sessionId, req);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.AuthRes,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
        private readonly PlayerRaidSessionService _playerRaidSessionService;
    }
}

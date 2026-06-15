using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class AuthPacketHandler : PacketHandlerBase<AuthReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.AuthReq;

        public AuthPacketHandler(SessionService sessionService, PlayerService playerService)
        {
            _sessionService = sessionService;
            _playerService = playerService;
        }

        protected override Task RunAsync(string sessionId, AuthReqPacket req)
        {
            var res = _playerService.Authenticate(sessionId, req);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.AuthRes, // TODO: 이런거 컨텐츠 단에서 넘기지않도록
                ProtocolType = EProtocolType.Json, // TODO: 이런거 컨텐츠 단에서 넘기지않도록
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
        private readonly PlayerService _playerService;
    }
}

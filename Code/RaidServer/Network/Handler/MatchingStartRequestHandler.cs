using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class MatchingStartRequestHandler : PacketHandlerBase<MatchingStartRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.MatchingStartRequest;
        public override bool RequireAuth => true;

        public MatchingStartRequestHandler(SessionService sessionService, MatchingService matchingService)
        {
            _sessionService = sessionService;
            _matchingService = matchingService;
        }

        protected override Task RunAsync(string sessionId, MatchingStartRequestPacket req)
        {
            var res = _matchingService.StartMatching(sessionId, req.BossNum);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.MatchingStartResponse,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
        private readonly MatchingService _matchingService;
    }
}

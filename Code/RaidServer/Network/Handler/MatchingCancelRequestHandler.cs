using Protocol.Raid;
using RaidServer.Services;

namespace RaidServer.Network
{
    public class MatchingCancelRequestHandler : PacketHandlerBase<MatchingCancelRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.MatchingCancelRequest;
        public override bool RequireAuth => true;

        public MatchingCancelRequestHandler(SessionService sessionService, MatchingService matchingService)
        {
            _sessionService = sessionService;
            _matchingService = matchingService;
        }

        protected override Task RunAsync(string sessionId, MatchingCancelRequestPacket req)
        {
            var res = _matchingService.CancelMatching(sessionId);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.MatchingCancelResponse,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
        private readonly MatchingService _matchingService;
    }
}

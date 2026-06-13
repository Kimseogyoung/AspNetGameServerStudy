using Protocol.Raid;

namespace RaidServer.Network
{
    public class PingPacketHandler : PacketHandlerBase<PingReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.PingReq;

        public PingPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, PingReqPacket req)
        {
            if (_sessionService.TryGetNetworkSession(sessionId, out var session))
            {
                session.LastActivityTime = DateTime.UtcNow;
            }

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.PongRes,
                ProtocolType = EProtocolType.Json,
                Payload = new PongResPacket { ServerTime = DateTime.UtcNow },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

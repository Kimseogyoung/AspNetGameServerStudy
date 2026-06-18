using Protocol.Raid;

namespace RaidServer.Network
{
    public class PingPacketHandler : PacketHandlerBase<PingRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.PingRequest;

        public PingPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, PingRequestPacket req)
        {
            if (_sessionService.TryGetNetworkSession(sessionId, out var session))
            {
                session.LastActivityTime = DateTime.UtcNow;
            }

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.PongResponse,
                ProtocolType = EProtocolType.Json,
                Payload = new PongResponsePacket { ServerTime = DateTime.UtcNow },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

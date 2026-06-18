using Protocol.Raid;

namespace RaidServer.Network
{
    public class EchoPacketHandler : PacketHandlerBase<EchoRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.EchoRequest;

        public EchoPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, EchoRequestPacket req)
        {
            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.EchoResponse,
                ProtocolType = EProtocolType.Json,
                Payload = new EchoResponsePacket { Message = req.Message },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

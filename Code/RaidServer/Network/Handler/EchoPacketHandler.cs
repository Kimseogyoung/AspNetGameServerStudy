using Protocol.Raid;

namespace RaidServer.Network
{
    public class EchoPacketHandler : PacketHandlerBase<EchoReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.EchoReq;

        public EchoPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, EchoReqPacket req)
        {
            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.EchoRes,
                ProtocolType = EProtocolType.Json,
                Payload = new EchoResPacket { Message = req.Message },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

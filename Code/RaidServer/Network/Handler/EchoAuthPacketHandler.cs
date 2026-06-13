using Protocol.Raid;

namespace RaidServer.Network
{
    public class EchoAuthPacketHandler : PacketHandlerBase<EchoReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.EchoAuthReq;
        public override bool RequireAuth => true;

        public EchoAuthPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, EchoReqPacket req)
        {
            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.EchoAuthRes,
                ProtocolType = EProtocolType.Json,
                Payload = new EchoResPacket { Message = req.Message },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

using Protocol.Raid;

namespace RaidServer.Network
{
    public class EchoAuthPacketHandler : PacketHandlerBase<EchoRequestPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.EchoAuthRequest;
        public override bool RequireAuth => true;

        public EchoAuthPacketHandler(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override Task RunAsync(string sessionId, EchoRequestPacket req)
        {
            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.EchoAuthResponse,
                ProtocolType = EProtocolType.Json,
                Payload = new EchoResponsePacket { Message = req.Message },
            });

            return Task.CompletedTask;
        }

        private readonly SessionService _sessionService;
    }
}

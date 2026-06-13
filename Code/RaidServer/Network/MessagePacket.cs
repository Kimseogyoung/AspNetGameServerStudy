using ServerCore.Serializer;

namespace RaidServer.Network
{
    // 송신 패킷: SessionService.Send/Broadcast에 전달하는, 아직 직렬화되지 않은 패킷
    public class MessagePacket
    {
        public required ushort Opcode { get; init; }
        public required EProtocolType ProtocolType { get; init; }
        public required object Payload { get; init; }
    }
}

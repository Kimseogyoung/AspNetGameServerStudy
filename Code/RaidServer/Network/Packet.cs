using ServerCore.Serializer;

namespace RaidServer.Network
{
    // 수신 패킷: PacketCodec.Parse가 헤더를 디코드한 결과 (Payload는 아직 역직렬화 전 byte[])
    public class Packet
    {
        public required string SessionId { get; init; }
        public required ushort Opcode { get; init; }
        public required EProtocolType ProtocolType { get; init; }
        public required byte[] Payload { get; init; }
    }
}

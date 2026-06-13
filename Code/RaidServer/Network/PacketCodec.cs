using System.Buffers.Binary;
using ServerCore.Serializer;

namespace RaidServer.Network
{
    // 패킷 프레임 포맷: [4B BE 길이][2B BE opcode][1B protocolType][... payload]
    // 길이(4B)는 opcode + protocolType + payload의 합산 크기 (길이 필드 자신은 제외)
    public static class PacketCodec
    {
        // bytes: SocketService가 길이(4B) 프리픽스를 이미 제거하고 넘긴 본문
        public static Packet Parse(string sessionId, byte[] bytes)
        {
            var opcode = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(0, 2));
            var protocolType = (EProtocolType)bytes[2];
            var payload = bytes[3..];

            return new Packet
            {
                SessionId = sessionId,
                Opcode = opcode,
                ProtocolType = protocolType,
                Payload = payload,
            };
        }

        // 길이(4B) 프리픽스까지 포함한 완성 프레임을 반환 (WriteLoop에서 그대로 전송)
        public static byte[] Encode(ushort opcode, EProtocolType protocolType, byte[] payloadBytes)
        {
            var bodyLength = 2 + 1 + payloadBytes.Length;
            var bytes = new byte[4 + bodyLength];

            BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(0, 4), bodyLength);
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(4, 2), opcode);
            bytes[6] = (byte)protocolType;
            payloadBytes.CopyTo(bytes.AsSpan(7));

            return bytes;
        }
    }
}

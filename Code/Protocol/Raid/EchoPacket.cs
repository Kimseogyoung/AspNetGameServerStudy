using ProtoBuf;

namespace Protocol.Raid
{
    [ProtoContract]
    public class EchoReqPacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class EchoResPacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;
    }
}

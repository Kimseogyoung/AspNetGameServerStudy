using ProtoBuf;

namespace Protocol.Raid
{
    [ProtoContract]
    public class EchoRequestPacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class EchoResponsePacket
    {
        [ProtoMember(1)]
        public string Message { get; set; } = string.Empty;
    }
}

using System;
using ProtoBuf;

namespace Protocol.Raid
{
    [ProtoContract]
    public class PingRequestPacket
    {
    }

    [ProtoContract]
    public class PongResponsePacket
    {
        [ProtoMember(1)]
        public DateTime ServerTime { get; set; }
    }
}

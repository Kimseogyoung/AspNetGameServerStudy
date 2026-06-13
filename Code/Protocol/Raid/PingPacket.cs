using System;
using ProtoBuf;

namespace Protocol.Raid
{
    [ProtoContract]
    public class PingReqPacket
    {
    }

    [ProtoContract]
    public class PongResPacket
    {
        [ProtoMember(1)]
        public DateTime ServerTime { get; set; }
    }
}

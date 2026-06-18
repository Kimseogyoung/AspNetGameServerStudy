using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public partial class RequestInfoPacket
    {

        [ProtoMember(1)]
        public long Seq { get; set; }

    }
}

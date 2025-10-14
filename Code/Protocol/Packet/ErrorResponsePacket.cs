using ProtoBuf;
namespace Protocol
{
    [ProtoContract]
    public class ErrorResponsePacket : IResPacket
    {
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
    }
}

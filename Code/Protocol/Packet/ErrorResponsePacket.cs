using ProtoBuf;
namespace Protocol
{
    [ProtoContract]
    public class ErrorResponsePacket : IResponsePacket
    {
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
    }
}

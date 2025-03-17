using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public KingdomDecoPacket KingdomDeco { get; set; } = new();
        
        [ProtoMember(3)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = new();
        
	}
}

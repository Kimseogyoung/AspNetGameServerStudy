using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStoreResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<KingdomStructurePacket> KingdomStructureList { get; set; } 
        
        [ProtoMember(3)]
        public List<KingdomDecoPacket> KingdomDecoList { get; set; } 
        
        [ProtoMember(4)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } 
        
	}
}

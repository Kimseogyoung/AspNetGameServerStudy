using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructStructureResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public KingdomStructurePacket KingdomStructure { get; set; } 
        
        [ProtoMember(3)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } 
        
        [ProtoMember(4)]
        public List<ObjPacket> CostObjList { get; set; } 
        
	}
}

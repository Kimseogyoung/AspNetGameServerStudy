using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyStructureResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public KingdomStructurePacket KingdomStructure { get; set; } 
        
        [ProtoMember(3)]
        public ObjPacket CostObj { get; set; } 
        
	}
}

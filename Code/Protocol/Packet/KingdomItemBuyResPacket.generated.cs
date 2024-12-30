using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomItemBuyResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public KingdomItemPacket KingdomItem { get; set; } 
        
        [ProtoMember(3)]
        public ObjPacket Obj { get; set; } 
        
	}
}

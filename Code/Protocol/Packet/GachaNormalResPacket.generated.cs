using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public CookiePacket Cookie { get; set; } 
        
        [ProtoMember(3)]
        public List<ChgObjPacket> GachaResultChgObjList { get; set; } 
        
        [ProtoMember(4)]
        public ChgObjPacket CostChgObj { get; set; } 
        
	}
}

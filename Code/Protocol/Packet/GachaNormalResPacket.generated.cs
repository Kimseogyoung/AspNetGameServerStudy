using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public List<ChgObjPacket> GachaResultChgObjList { get; set; } = new List<ChgObjPacket>();
        
        [ProtoMember(3)]
        public List<GachaResultPacket> GachaResultList { get; set; } = new List<GachaResultPacket>();
        
        [ProtoMember(4)]
        public ChgObjPacket CostChgObj { get; set; } = new ChgObjPacket();
        
	}
}

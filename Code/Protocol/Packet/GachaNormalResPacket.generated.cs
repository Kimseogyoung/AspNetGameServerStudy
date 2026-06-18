using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalResPacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public List<ChgObjPacket> GachaResultChgObjList { get; set; } = new List<ChgObjPacket>();
        
        [ProtoMember(3)]
        public List<GachaResultPacket> GachaResultList { get; set; } = new List<GachaResultPacket>();
        
        [ProtoMember(4)]
        public ChgObjPacket CostChgObj { get; set; } = new ChgObjPacket();
        
	}
}

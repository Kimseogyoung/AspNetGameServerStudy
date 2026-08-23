using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public List<CookiePacket> CookieList { get; set; } = new List<CookiePacket>();
        
        [ProtoMember(3)]
        public List<GachaResultPacket> GachaResultList { get; set; } = new List<GachaResultPacket>();
        
        [ProtoMember(4)]
        public List<ChgObjPacket> CostChgObjList { get; set; } = new List<ChgObjPacket>();
        
	}
}

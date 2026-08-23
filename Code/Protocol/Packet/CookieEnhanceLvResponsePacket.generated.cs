using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceLvResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public CookiePacket Cookie { get; set; } = new CookiePacket();
        
        [ProtoMember(3)]
        public List<ChgObjPacket> ChgObjList { get; set; } = new List<ChgObjPacket>();
        
	}
}

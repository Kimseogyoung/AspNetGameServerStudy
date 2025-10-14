using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceLvResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public CookiePacket Cookie { get; set; } = new CookiePacket();
        
        [ProtoMember(3)]
        public ChgObjPacket ChgObj { get; set; } = new ChgObjPacket();
        
	}
}

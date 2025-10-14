using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyDecoResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public KingdomDecoPacket KingdomDeco { get; set; } = new KingdomDecoPacket();
        
        [ProtoMember(3)]
        public ChgObjPacket ChgObj { get; set; } = new ChgObjPacket();
        
	}
}

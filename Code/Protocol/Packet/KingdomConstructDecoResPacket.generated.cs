using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public KingdomDecoPacket KingdomDeco { get; set; } = new KingdomDecoPacket();
        
        [ProtoMember(3)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = new List<PlacedKingdomItemPacket>();
        
	}
}

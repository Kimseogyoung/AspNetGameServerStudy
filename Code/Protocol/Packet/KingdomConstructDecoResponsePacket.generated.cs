using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public KingdomDecoPacket KingdomDeco { get; set; } = new KingdomDecoPacket();
        
        [ProtoMember(3)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = new List<PlacedKingdomItemPacket>();
        
	}
}

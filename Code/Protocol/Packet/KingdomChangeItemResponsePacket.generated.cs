using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomChangeItemResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = new List<PlacedKingdomItemPacket>();
        
        [ProtoMember(3)]
        public List<KingdomStructurePacket> KingdomStructureList { get; set; } = new List<KingdomStructurePacket>();
        
        [ProtoMember(4)]
        public List<KingdomDecoPacket> KingdomDecoList { get; set; } = new List<KingdomDecoPacket>();
        
	}
}

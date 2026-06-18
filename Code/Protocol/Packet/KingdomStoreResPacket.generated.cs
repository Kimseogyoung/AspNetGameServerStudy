using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStoreResPacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public List<KingdomStructurePacket> KingdomStructureList { get; set; } = new List<KingdomStructurePacket>();
        
        [ProtoMember(3)]
        public List<KingdomDecoPacket> KingdomDecoList { get; set; } = new List<KingdomDecoPacket>();
        
        [ProtoMember(4)]
        public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = new List<PlacedKingdomItemPacket>();
        
	}
}

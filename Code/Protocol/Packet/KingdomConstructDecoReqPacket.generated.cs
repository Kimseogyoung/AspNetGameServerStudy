using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(3)]
        public TilePosPacket StartTilePos { get; set; } 
        
        [ProtoMember(4)]
        public TilePosPacket EndTilePos { get; set; } 
        
        public string GetProtocolName() => "kingdom/construct-deco";
	}
}

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
        public int StartTileX { get; set; } 
        
        [ProtoMember(4)]
        public int StartTileY { get; set; } 
        
        [ProtoMember(5)]
        public int EndTileX { get; set; } 
        
        [ProtoMember(6)]
        public int EndTileY { get; set; } 
        
        public string GetProtocolName() => "kingdom/construct-deco";
	}
}

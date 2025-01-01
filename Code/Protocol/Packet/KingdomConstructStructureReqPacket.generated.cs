using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } 
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(4)]
        public List<CostObjPacket> CostObjList { get; set; } 
        
        [ProtoMember(5)]
        public int StartTileX { get; set; } 
        
        [ProtoMember(6)]
        public int StartTileY { get; set; } 
        
        [ProtoMember(7)]
        public int EndTileX { get; set; } 
        
        [ProtoMember(8)]
        public int EndTileY { get; set; } 
        
        public string GetProtocolName() => "kingdom/construct-structure";
	}
}

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
        public TilePosPacket StartTilePos { get; set; } 
        
        public string GetProtocolName() => "kingdom/construct-structure";

        public KingdomConstructStructureReqPacket( ReqInfoPacket info,  ulong kingdomstructureid,  int kingdomitemnum,  List<CostObjPacket> costobjlist,  TilePosPacket starttilepos )
	    {   
         
                Info = info; 
                 
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                CostObjList = costobjlist; 
                 
                StartTilePos = starttilepos; 
                
	    }
	}
}

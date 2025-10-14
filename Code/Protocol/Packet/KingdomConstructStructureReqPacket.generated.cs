using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } = default;
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(4)]
        public List<CostObjPacket> CostObjList { get; set; } = new List<CostObjPacket>();
        
        [ProtoMember(5)]
        public TilePosPacket StartTilePos { get; set; } = new TilePosPacket();
        

        public const string NAME = "kingdom/construct-structure";
        public string GetProtocolName() => NAME;

        public KingdomConstructStructureReqPacket( ulong kingdomstructureid,  int kingdomitemnum,  List<CostObjPacket> costobjlist,  TilePosPacket starttilepos )
	    {   
         
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                CostObjList = costobjlist; 
                 
                StartTilePos = starttilepos; 
                
	    }

    
        public KingdomConstructStructureReqPacket()
        {
        }
        

	}
}

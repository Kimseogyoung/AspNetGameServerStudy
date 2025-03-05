using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(3)]
        public CostObjPacket CostObj { get; set; } 
        
        public string GetProtocolName() => "kingdom/buy-structure";

        public KingdomBuyStructureReqPacket( ReqInfoPacket info,  int kingdomitemnum,  CostObjPacket costobj )
	    {   
         
                Info = info; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                CostObj = costobj; 
                
	    }
	}
}

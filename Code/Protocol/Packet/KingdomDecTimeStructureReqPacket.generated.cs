using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomDecTimeStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } 
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(4)]
        public int RemainSec { get; set; } 
        
        [ProtoMember(5)]
        public CostCashPacket CashCost { get; set; } 
        

        public const string NAME = "kingdom/dec-time-structure";
        public string GetProtocolName() => NAME;

        public KingdomDecTimeStructureReqPacket( ulong kingdomstructureid,  int kingdomitemnum,  int remainsec,  CostCashPacket cashcost )
	    {   
         
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                RemainSec = remainsec; 
                 
                CashCost = cashcost; 
                
	    }

    
        public KingdomDecTimeStructureReqPacket()
        {
        }
        

	}
}

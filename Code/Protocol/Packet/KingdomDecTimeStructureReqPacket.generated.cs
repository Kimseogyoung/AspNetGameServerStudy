using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomDecTimeStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } = default;
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(4)]
        public int RemainSec { get; set; } = default;
        
        [ProtoMember(5)]
        public CostCashPacket CashCost { get; set; } = new CostCashPacket();
        

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

using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomDecTimeStructureRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
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

        public KingdomDecTimeStructureRequestPacket( ulong kingdomstructureid,  int kingdomitemnum,  int remainsec,  CostCashPacket cashcost )
	    {   
         
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                RemainSec = remainsec; 
                 
                CashCost = cashcost; 
                
	    }

    
        public KingdomDecTimeStructureRequestPacket()
        {
        }
        

	}
}

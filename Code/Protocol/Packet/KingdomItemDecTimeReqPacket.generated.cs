using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomItemDecTimeReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(3)]
        public int RemainSec { get; set; } 
        
        [ProtoMember(4)]
        public CostCashPacket CashCost { get; set; } 
        
        public string GetProtocolName() => "kingdom-item/dec-time";
	}
}

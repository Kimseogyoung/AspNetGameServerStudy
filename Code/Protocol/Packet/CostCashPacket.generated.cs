using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CostCashPacket	
        {
        
                [ProtoMember(1)]
                public long BefAmount { get; set; } 
                
                [ProtoMember(2)]
                public long AftAmount { get; set; } 
                
                [ProtoMember(3)]
                public long Amount { get; set; } 
                
	}
}

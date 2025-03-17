using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CostCashPacket	
        {
        
                [ProtoMember(1)]
                public double BefAmount { get; set; } = default;
                
                [ProtoMember(2)]
                public double AftAmount { get; set; } = default;
                
                [ProtoMember(3)]
                public double Amount { get; set; } = default;
                
	}
}

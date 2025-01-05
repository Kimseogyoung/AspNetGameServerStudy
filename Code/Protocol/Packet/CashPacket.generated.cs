using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CashPacket	
        {
        
                [ProtoMember(1)]
                public double FreeCash { get; set; } 
                
                [ProtoMember(2)]
                public double RealCash { get; set; } 
                
	}
}

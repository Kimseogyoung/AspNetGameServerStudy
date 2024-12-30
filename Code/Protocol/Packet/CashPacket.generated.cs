using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CashPacket	
        {
        
                [ProtoMember(1)]
                public long FreeCash { get; set; } 
                
                [ProtoMember(2)]
                public long RealCash { get; set; } 
                
	}
}

using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class PlayerDetailPacket
	{
    
    		[ProtoMember(1)]
    		public double Gold { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public double StarCandy { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public double RealCash { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public double FreeCash { get; set; } = default; //
        
	}
}

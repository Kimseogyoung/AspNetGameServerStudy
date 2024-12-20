using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class PointPacket
	{
    
    		[ProtoMember(0)]
    		public ulong PlayerId { get; set; } = default; //
        
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public double Amount { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public double AccAmount { get; set; } = default; //
        
	}
}

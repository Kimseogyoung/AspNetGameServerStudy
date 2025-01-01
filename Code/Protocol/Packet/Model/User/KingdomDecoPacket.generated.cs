using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomDecoPacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int TotalCnt { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int UnplacedCnt { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public EKingdomItemState State { get; set; } = default; //
        
	}
}

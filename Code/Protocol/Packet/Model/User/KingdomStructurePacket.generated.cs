using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStructurePacket
	{
    
    		[ProtoMember(1)]
    		public ulong Id { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public EKingdomItemState State { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public ulong Flag { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

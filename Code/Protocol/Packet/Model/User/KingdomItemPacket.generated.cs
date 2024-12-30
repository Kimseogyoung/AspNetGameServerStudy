using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomItemPacket
	{
    
    		[ProtoMember(1)]
    		public EKingdomItemType Type { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public EKingdomItemState State { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public DateTime EndTime { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public int StartTileX { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public int StartTileY { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public int EndTileY { get; set; } = default; //
        
	}
}

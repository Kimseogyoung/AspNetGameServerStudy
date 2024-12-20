using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomObjPacket
	{
    
    		[ProtoMember(2)]
    		public EKingdomObjType Type { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public EKingdomObjState State { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public DateTime EndTime { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public int StartTileX { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public int StartTileY { get; set; } = default; //
        
    		[ProtoMember(9)]
    		public int EndTileY { get; set; } = default; //
        
	}
}

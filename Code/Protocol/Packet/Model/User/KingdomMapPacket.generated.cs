using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomMapPacket
	{
    
    		[ProtoMember(1)]
    		public int XSize { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int Ysize { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string Snapshot { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public EKingdomTileMapState State { get; set; } = default; //
        
	}
}

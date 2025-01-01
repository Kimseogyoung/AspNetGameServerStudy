using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomTileMapPacket
	{
    
    		[ProtoMember(1)]
    		public string Snapshot { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public EKingdomTileMapState State { get; set; } = default; //
        
	}
}

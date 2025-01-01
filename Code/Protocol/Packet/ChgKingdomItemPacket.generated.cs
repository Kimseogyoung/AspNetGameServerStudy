using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ChgKingdomItemPacket	
        {
        
                [ProtoMember(1)]
                public ulong PlacedItemId { get; set; } 
                
                [ProtoMember(2)]
                public int Num { get; set; } 
                
                [ProtoMember(3)]
                public TilePosPacket TilePos { get; set; } 
                
	}
}

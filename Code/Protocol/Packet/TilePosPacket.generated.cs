using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class TilePosPacket	
        {
        
                [ProtoMember(1)]
                public int X { get; set; } 
                
                [ProtoMember(2)]
                public int Y { get; set; } 
                
	}
}

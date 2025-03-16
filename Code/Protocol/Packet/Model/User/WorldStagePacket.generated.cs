using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldStagePacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int WorldNum { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int Star { get; set; } = default; //
        
	}
}

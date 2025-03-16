using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldPacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int TopFinishStageOrder { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int TopFinishStageNum { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public int LastPlayStageNum { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public int RecvStarReward { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public ulong Flag { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public int State { get; set; } = default; //
        
	}
}

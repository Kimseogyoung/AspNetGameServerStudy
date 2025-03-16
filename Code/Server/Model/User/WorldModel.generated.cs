using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class WorldModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public int TopFinishStageOrder { get; set; } = default; //
        
    		
    		public int TopFinishStageNum { get; set; } = default; //
        
    		
    		public int LastPlayStageNum { get; set; } = default; //
        
    		
    		public int RecvStarReward { get; set; } = default; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public int State { get; set; } = default; //
        
	}
}

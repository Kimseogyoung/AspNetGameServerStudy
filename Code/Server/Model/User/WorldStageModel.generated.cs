using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class WorldStageModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public int WorldId { get; set; } = default; //
        
    		
    		public int Star { get; set; } = default; //
        
	}
}

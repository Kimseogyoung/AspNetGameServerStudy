using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class PlayerMapModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public int ShardId { get; set; } = default; //
        
    		
    		public DateTime UpdateTime { get; set; } = default; //
        
    		
    		public DateTime CreateTime { get; set; } = default; //
        
	}
}

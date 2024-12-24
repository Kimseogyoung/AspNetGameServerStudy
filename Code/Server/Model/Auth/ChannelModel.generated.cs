using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class ChannelModel : ModelBase
	{
    
    		
    		public string Key { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public EChannelType Type { get; set; } = default; //
        
    		
    		public string Token { get; set; } = default; //
        
    		
    		public EChannelState State { get; set; } = default; //
        
    		
    		public DateTime UpdateTime { get; set; } = default; //
        
    		
    		public DateTime CreateTime { get; set; } = default; //
        
	}
}

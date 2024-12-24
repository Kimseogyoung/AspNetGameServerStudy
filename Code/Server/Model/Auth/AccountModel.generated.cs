using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class AccountModel : ModelBase
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public int ShardId { get; set; } = default; //
        
    		
    		public EAccountState State { get; set; } = default; //
        
    		
    		public string ClientSecret { get; set; } = default; //
        
    		
    		public int AdditionalPlayerCnt { get; set; } = default; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public int Age { get; set; } = default; //
        
    		
    		public DateTime UpdateTime { get; set; } = default; //
        
    		
    		public DateTime CreateTime { get; set; } = default; //
        
	}
}

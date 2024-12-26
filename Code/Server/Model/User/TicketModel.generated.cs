using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class TicketModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public double Amount { get; set; } = default; //
        
    		
    		public double AccAmount { get; set; } = default; //
        
    		
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

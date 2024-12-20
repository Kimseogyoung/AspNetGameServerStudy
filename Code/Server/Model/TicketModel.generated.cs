using ProtoBuf;
using Proto;
namespace Protocol
{
	
	public partial class TicketModel
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public double Amount { get; set; } = default; //
        
    		
    		public double AccAmount { get; set; } = default; //
        
    		
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

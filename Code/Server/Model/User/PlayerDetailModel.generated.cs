using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class PlayerDetailModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public double Exp { get; set; } = default; //
        
    		
    		public double AccExp { get; set; } = default; //
        
    		
    		public double Gold { get; set; } = default; //
        
    		
    		public double AccGold { get; set; } = default; //
        
    		
    		public double RealCash { get; set; } = default; //
        
    		
    		public double FreeCash { get; set; } = default; //
        
    		
    		public double AccRealCash { get; set; } = default; //
        
    		
    		public double AccFreeCash { get; set; } = default; //
        
	}
}

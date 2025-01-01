using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomDecoModel : ModelBase
	{
    
    		
    		public int Num { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int TotalCnt { get; set; } = default; //
        
    		
    		public int UnplacedCnt { get; set; } = default; //
        
    		
    		public EKingdomItemState State { get; set; } = default; //
        
	}
}

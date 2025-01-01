using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomStructureModel : ModelBase
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public EKingdomItemState State { get; set; } = default; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

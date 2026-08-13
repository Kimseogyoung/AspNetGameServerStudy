using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["SfId"], ScopeKey = "PlayerId")]
	public partial class KingdomStructureModel : ModelBase
	{
    
    		
    		public ulong SfId { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public EKingdomItemState State { get; set; } = default; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

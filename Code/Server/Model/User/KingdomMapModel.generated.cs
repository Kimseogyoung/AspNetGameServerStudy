using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomMapModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int XSize { get; set; } = default; //
        
    		
    		public int Ysize { get; set; } = default; //
        
    		
    		public string Snapshot { get; set; } = default; //
        
    		
    		public EKingdomTileMapState State { get; set; } = default; //
        
	}
}

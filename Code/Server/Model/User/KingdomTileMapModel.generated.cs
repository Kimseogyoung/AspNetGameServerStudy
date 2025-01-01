using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomTileMapModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public string Snapshot { get; set; } = default; //
        
    		
    		public EKingdomTileMapState State { get; set; } = default; //
        
	}
}

using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomMapModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int SizeX { get; set; } = default; //
        
    		
    		public int SizeY { get; set; } = default; //
        
    		
    		public string Snapshot { get; set; } = default; //
        
    		
    		public EKingdomTileMapState State { get; set; } = default; //
        
	}
}

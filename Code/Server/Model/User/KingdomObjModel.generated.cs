using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class KingdomObjModel : ModelBase
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public EKingdomObjType Type { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public EKingdomObjState State { get; set; } = default; //
        
    		
    		public DateTime EndTime { get; set; } = default; //
        
    		
    		public int StartTileX { get; set; } = default; //
        
    		
    		public int StartTileY { get; set; } = default; //
        
    		
    		public int EndTileX { get; set; } = default; //
        
    		
    		public int EndTileY { get; set; } = default; //
        
	}
}

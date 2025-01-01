using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class PlacedKingdomItemModel : ModelBase
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public ulong StructureItemId { get; set; } = default; //
        
    		
    		public EKingdomItemType Type { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public EPlacedKingdomItemState State { get; set; } = default; //
        
    		
    		public int StartTileX { get; set; } = default; //
        
    		
    		public int StartTileY { get; set; } = default; //
        
    		
    		public int SizeX { get; set; } = default; //
        
    		
    		public int SizeY { get; set; } = default; //
        
    		
    		public int Rotation { get; set; } = default; //
        
	}
}

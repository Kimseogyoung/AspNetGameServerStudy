using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["PlayerId", "Num"], ScopeKey = "PlayerId")]
	public partial class ItemModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public EItemType Type { get; set; } = default; //
        
    		
    		public double Amount { get; set; } = default; //
        
    		
    		public double AccAmount { get; set; } = default; //
        
	}
}

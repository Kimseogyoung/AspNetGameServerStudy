using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["SfId"], ScopeKey = "PlayerId")]
	public partial class CashChangeLogModel : ModelBase
	{
    
    		
    		public ulong SfId { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int ActionNameHash { get; set; } = default; //
        
    		
    		public string ActionName { get; set; } = default; //
        
    		
    		public string ActionDetail { get; set; } = default; //
        
    		
    		public long ChgRealCash { get; set; } = default; //
        
    		
    		public long BefRealCash { get; set; } = default; //
        
    		
    		public long AftRealCash { get; set; } = default; //
        
    		
    		public long AccRealCash { get; set; } = default; //
        
    		
    		public long ChgFreeCash { get; set; } = default; //
        
    		
    		public long BefFreeCash { get; set; } = default; //
        
    		
    		public long AftFreeCash { get; set; } = default; //
        
    		
    		public long AccFreeCash { get; set; } = default; //
        
    		
    		public long ChgTotalCash { get; set; } = default; //
        
    		
    		public long BefTotalCash { get; set; } = default; //
        
    		
    		public long AftTotalCash { get; set; } = default; //
        
    		
    		public long AccTotalCash { get; set; } = default; //
        
    		
    		public ulong IapActionId { get; set; } = default; //
        
	}
}

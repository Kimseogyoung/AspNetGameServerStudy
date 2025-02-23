using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class GachaLogModel : ModelBase
	{
    
    		
    		public ulong SfId { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int ScheduleNum { get; set; } = default; //
        
    		
    		public int Cnt { get; set; } = default; //
        
    		
    		public int ChgRealCash { get; set; } = default; //
        
    		
    		public int ChgFreeCash { get; set; } = default; //
        
    		
    		public EObjType ChgObjType { get; set; } = default; //
        
    		
    		public int ChgObjAmount { get; set; } = default; //
        
    		
    		public string ExtraData { get; set; } = default; //
        
	}
}

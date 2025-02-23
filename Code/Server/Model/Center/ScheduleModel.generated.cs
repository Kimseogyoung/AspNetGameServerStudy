using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class ScheduleModel : ModelBase
	{
    
    		
    		public int Num { get; set; } = default; //
        
    		
    		public DateTime ActiveStartTime { get; set; } = default; //
        
    		
    		public DateTime ActiveEndTime { get; set; } = default; //
        
    		
    		public DateTime ContentStartTime { get; set; } = default; //
        
    		
    		public DateTime ContentEndTime { get; set; } = default; //
        
    		
    		public int State { get; set; } = default; //
        
	}
}

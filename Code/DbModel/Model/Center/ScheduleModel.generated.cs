using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["Num"])]
	public partial class ScheduleModel : ModelBase
	{
    
    		
    		public int Num { get; set; } = default; //
        
    		
    		public DateTime ActiveStartTime { get; set; } = default; //
        
    		
    		public DateTime ActiveEndTime { get; set; } = default; //
        
    		
    		public DateTime ContentStartTime { get; set; } = default; //
        
    		
    		public DateTime ContentEndTime { get; set; } = default; //
        
    		
    		public int State { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is ScheduleModel otherModel
				&& Num == otherModel.Num;
		}
	}
}

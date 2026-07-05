using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class ScheduleProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public EScheduleType Type { get; set; }
        
    		public DateTime ActiveStartTime { get; set; }
        
    		public DateTime ContentStartTime { get; set; }
        
    		public DateTime ContentEndTime { get; set; }
        
    		public DateTime ActiveEndTime { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Type": Type = Enum.Parse<EScheduleType>(value); break;
        
        
        
    				case "ActiveStartTime": ActiveStartTime = DateTime.Parse(value); break;
        
        
        
    				case "ContentStartTime": ContentStartTime = DateTime.Parse(value); break;
        
        
        
    				case "ContentEndTime": ContentEndTime = DateTime.Parse(value); break;
        
        
        
    				case "ActiveEndTime": ActiveEndTime = DateTime.Parse(value); break;
        
        
			}
		}
	}
}

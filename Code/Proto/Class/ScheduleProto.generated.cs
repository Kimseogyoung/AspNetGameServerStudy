using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class ScheduleProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public EScheduleType Type { get; set; }
        
    		[ProtoMember(4)]
    		public DateTime ActiveStartTime { get; set; }
        
    		[ProtoMember(5)]
    		public DateTime ContentStartTime { get; set; }
        
    		[ProtoMember(6)]
    		public DateTime ConentEndTime { get; set; }
        
    		[ProtoMember(7)]
    		public DateTime ActiveEndTime { get; set; }
        
	}
}

using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class SchedulePacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public DateTime ActiveStartTime { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public DateTime ActiveEndTime { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public DateTime ContentStartTime { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public DateTime ContentEndTime { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public int State { get; set; } = default; //
        
	}
}

using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class TicketPacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public double Amount { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public double AccAmount { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public DateTime EndTime { get; set; } = default; //
        
	}
}

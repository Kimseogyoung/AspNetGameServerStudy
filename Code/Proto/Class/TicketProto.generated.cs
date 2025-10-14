using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class TicketProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public EObjType Type { get; set; }
        
    		[ProtoMember(3)]
    		public string Name { get; set; }
        
    		[ProtoMember(4)]
    		public int ChargeSec { get; set; }
        
    		[ProtoMember(5)]
    		public int ChargeAmount { get; set; }
        
    		[ProtoMember(6)]
    		public int MaxAmount { get; set; }
        
	}
}

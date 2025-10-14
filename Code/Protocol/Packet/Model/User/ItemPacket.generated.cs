using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class ItemPacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public EItemType Type { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public double Amount { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public double AccAmount { get; set; } = default; //
        
	}
}

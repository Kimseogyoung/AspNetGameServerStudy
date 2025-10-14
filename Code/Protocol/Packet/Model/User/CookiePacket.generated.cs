using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class CookiePacket
	{
    
    		[ProtoMember(1)]
    		public int Num { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int SoulStone { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int AccSoulStone { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public int Star { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public int Lv { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public int SkillLv { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public int Flag { get; set; } = default; //
        
    		[ProtoMember(8)]
    		public ECookieState State { get; set; } = default; //
        
	}
}

using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class CookieStarEnhanceProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public EGradeType Type { get; set; }
        
    		[ProtoMember(3)]
    		public int Star { get; set; }
        
    		[ProtoMember(4)]
    		public int SoulStone { get; set; }
        
	}
}

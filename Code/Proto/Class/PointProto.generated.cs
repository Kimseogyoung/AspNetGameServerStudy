using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class PointProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public EObjType Type { get; set; }
        
    		[ProtoMember(3)]
    		public string Name { get; set; }
        
	}
}

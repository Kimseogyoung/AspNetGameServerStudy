using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class IconProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public string Key { get; set; }
        
    		[ProtoMember(3)]
    		public string Sprite { get; set; }
        
	}
}

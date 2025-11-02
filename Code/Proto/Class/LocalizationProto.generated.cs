using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class LocalizationProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public string Key { get; set; }
        
    		[ProtoMember(3)]
    		public string ko { get; set; }
        
    		[ProtoMember(4)]
    		public string en { get; set; }
        
	}
}

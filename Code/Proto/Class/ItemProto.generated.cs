using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class ItemProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public EItemType Type { get; set; }
        
    		[ProtoMember(4)]
    		public string NameKey { get; set; }
        
    		[ProtoMember(5)]
    		public string Name { get; set; }
        
    		[ProtoMember(6)]
    		public int DisplayOrder { get; set; }
        
    		[ProtoMember(7)]
    		public EObjType SaleObjType { get; set; }
        
    		[ProtoMember(8)]
    		public int SaleObjNum { get; set; }
        
    		[ProtoMember(9)]
    		public int SaleObjAmount { get; set; }
        
    		[ProtoMember(10)]
    		public string Sprite { get; set; }
        
	}
}

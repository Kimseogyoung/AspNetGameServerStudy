using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class GachaScheduleProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public string Tag { get; set; }
        
    		[ProtoMember(4)]
    		public int DisplayOrder { get; set; }
        
    		[ProtoMember(5)]
    		public int Seq { get; set; }
        
    		[ProtoMember(6)]
    		public string Name { get; set; }
        
    		[ProtoMember(7)]
    		public int GachaProbNum { get; set; }
        
    		[ProtoMember(8)]
    		public List<int> PickupCookieNumList { get; set; }
        
    		[ProtoMember(13)]
    		public List<EObjType> CostTypeList { get; set; }
        
    		[ProtoMember(14)]
    		public List<int> CostAmountList { get; set; }
        
    		[ProtoMember(17)]
    		public List<int> CntList { get; set; }
        
	}
}

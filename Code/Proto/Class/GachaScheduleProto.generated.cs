using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class GachaScheduleProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(4)]
    		public string Tag { get; set; }
        
    		[ProtoMember(5)]
    		public int Order { get; set; }
        
    		[ProtoMember(6)]
    		public int DisplayOrder { get; set; }
        
    		[ProtoMember(7)]
    		public string Name { get; set; }
        
    		[ProtoMember(8)]
    		public int GachaProbNum { get; set; }
        
    		[ProtoMember(9)]
    		public int PickupCookieNum { get; set; }
        
    		[ProtoMember(10)]
    		public List<EObjType> CostTypeList { get; set; }
        
    		[ProtoMember(11)]
    		public List<int> CostAmountList { get; set; }
        
    		[ProtoMember(14)]
    		public List<int> CntList { get; set; }
        
	}
}

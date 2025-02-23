using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class GachaProbProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public List<int> GradeWeightList { get; set; }
        
    		[ProtoMember(9)]
    		public List<int> PickupWeightList { get; set; }
        
    		[ProtoMember(12)]
    		public int WeightSum { get; set; }
        
    		[ProtoMember(13)]
    		public int CookieWeight { get; set; }
        
    		[ProtoMember(14)]
    		public int SoulStoneWeight { get; set; }
        
    		[ProtoMember(15)]
    		public int DetailWeightSum { get; set; }
        
    		[ProtoMember(16)]
    		public int SoulStoneMinCnt { get; set; }
        
    		[ProtoMember(17)]
    		public int SoulStoneMaxCnt { get; set; }
        
	}
}

using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class KingdomItemProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public string Name { get; set; }
        
    		[ProtoMember(4)]
    		public EKingdomItemType Type { get; set; }
        
    		[ProtoMember(5)]
    		public EKingdomItemSpecialType SpecialType { get; set; }
        
    		[ProtoMember(6)]
    		public string SizeX { get; set; }
        
    		[ProtoMember(7)]
    		public string SizeY { get; set; }
        
    		[ProtoMember(8)]
    		public int MaxCnt { get; set; }
        
    		[ProtoMember(9)]
    		public int MaxLv { get; set; }
        
    		[ProtoMember(10)]
    		public int CastleLv { get; set; }
        
    		[ProtoMember(11)]
    		public int ConstructSec { get; set; }
        
    		[ProtoMember(12)]
    		public EObjType ConstructObjType { get; set; }
        
    		[ProtoMember(13)]
    		public int ConstructObjNum { get; set; }
        
    		[ProtoMember(14)]
    		public int ConstructObjAmount { get; set; }
        
    		[ProtoMember(15)]
    		public EObjType CostObjType { get; set; }
        
    		[ProtoMember(16)]
    		public int CostObjNum { get; set; }
        
    		[ProtoMember(17)]
    		public int CostObjAmount { get; set; }
        
    		[ProtoMember(18)]
    		public int ProductionSec { get; set; }
        
    		[ProtoMember(19)]
    		public EObjType ProductObjType { get; set; }
        
    		[ProtoMember(20)]
    		public int ProductObjNum { get; set; }
        
    		[ProtoMember(21)]
    		public int ProductObjAmount { get; set; }
        
    		[ProtoMember(22)]
    		public int DecoPoint { get; set; }
        
	}
}

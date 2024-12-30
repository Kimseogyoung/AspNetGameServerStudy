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
    		public int ProductionSec { get; set; }
        
    		[ProtoMember(9)]
    		public EObjType ProductObjType { get; set; }
        
    		[ProtoMember(10)]
    		public string ProductObjNum { get; set; }
        
    		[ProtoMember(11)]
    		public string ProductObjAmount { get; set; }
        
    		[ProtoMember(12)]
    		public int MaxLv { get; set; }
        
	}
}

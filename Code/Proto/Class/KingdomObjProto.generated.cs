using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class KingdomObjProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public string Name { get; set; }
        
    		[ProtoMember(4)]
    		public EKingdomObjType Type { get; set; }
        
    		[ProtoMember(5)]
    		public EKingdomObjSpecialType SpecialType { get; set; }
        
    		[ProtoMember(6)]
    		public string SizeX { get; set; }
        
    		[ProtoMember(7)]
    		public string SizeY { get; set; }
        
    		[ProtoMember(8)]
    		public int ProductionSec { get; set; }
        
    		[ProtoMember(9)]
    		public EObjType ProductionType { get; set; }
        
    		[ProtoMember(10)]
    		public string ProductionNum { get; set; }
        
    		[ProtoMember(11)]
    		public string ProductionAmount { get; set; }
        
    		[ProtoMember(12)]
    		public int MaxLv { get; set; }
        
	}
}

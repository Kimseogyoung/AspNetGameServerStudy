using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class WorldProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public EWorldType Type { get; set; }
        
    		[ProtoMember(4)]
    		public int Order { get; set; }
        
    		[ProtoMember(5)]
    		public string Name { get; set; }
        
    		[ProtoMember(6)]
    		public List<int> RewardStarList { get; set; }
        
    		[ProtoMember(7)]
    		public List<int> RewardStarCashList { get; set; }
        
	}
}

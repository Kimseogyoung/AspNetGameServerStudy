using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class WorldStageProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public int WorldNum { get; set; }
        
    		[ProtoMember(4)]
    		public int Order { get; set; }
        
    		[ProtoMember(5)]
    		public int Lv { get; set; }
        
    		[ProtoMember(6)]
    		public EWorldStageType Type { get; set; }
        
    		[ProtoMember(7)]
    		public string Name { get; set; }
        
    		[ProtoMember(8)]
    		public int BossNum { get; set; }
        
    		[ProtoMember(9)]
    		public int SteminaCnt { get; set; }
        
    		[ProtoMember(10)]
    		public List<EObjType> FirstRewardTypeList { get; set; }
        
    		[ProtoMember(11)]
    		public List<int> FirstRewardNumList { get; set; }
        
    		[ProtoMember(12)]
    		public List<int> FirstRewardAmountList { get; set; }
        
	}
}

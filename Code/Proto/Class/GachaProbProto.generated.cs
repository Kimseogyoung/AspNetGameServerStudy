using ProtoBuf;
using System.Collections.Generic;
using System;
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
        
    		[ProtoMember(14)]
    		public int WeightSum { get; set; }
        
    		[ProtoMember(15)]
    		public List<int> DetailWeightList { get; set; }
        
    		[ProtoMember(19)]
    		public int DetailWeightSum { get; set; }
        
	}
}

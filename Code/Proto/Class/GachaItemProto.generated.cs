using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class GachaItemProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public EGachaItemType Type { get; set; }
        
    		[ProtoMember(4)]
    		public string Tag { get; set; }
        
    		[ProtoMember(5)]
    		public int Seq { get; set; }
        
	}
}

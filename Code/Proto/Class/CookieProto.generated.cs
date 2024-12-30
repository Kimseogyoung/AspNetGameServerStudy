using ProtoBuf;
namespace Proto
{
	[ProtoContract]
	public partial class CookieProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public string Name { get; set; }
        
	}
}

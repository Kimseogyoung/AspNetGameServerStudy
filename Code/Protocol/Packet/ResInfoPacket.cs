using ProtoBuf;

namespace Protocol
{
	[ProtoContract]
	public partial class ResInfoPacket
	{
    
    		[ProtoMember(1)]
    		public int ResultCode { get; set; }
        
    		[ProtoMember(2)]
    		public string ResultMsg { get; set; }
        
	}
}

using ProtoBuf;

namespace Protocol
{
	[ProtoContract]
	public partial class ReqInfoPacket
	{
    
    		[ProtoMember(1)]
    		public long Seq { get; set; }
        
	}
}

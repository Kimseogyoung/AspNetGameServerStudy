using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ChannelPacket
	{
    
    		[ProtoMember(1)]
    		public string Key { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public EChannelType Type { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string Token { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public EChannelState State { get; set; } = default; //
        
	}
}

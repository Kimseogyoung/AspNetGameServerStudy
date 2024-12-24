using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class AccountPacket
	{
    
    		[ProtoMember(1)]
    		public int ShardId { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public EAccountState State { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string ClientSecret { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public int AdditionalPlayerCnt { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public ulong Flag { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public int Age { get; set; } = default; //
        
	}
}

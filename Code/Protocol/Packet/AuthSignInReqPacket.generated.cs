using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignInReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public string ChannelId { get; set; } 
        
        public string GetProtocolName() => "auth/sign-in";

        public AuthSignInReqPacket( string channelid )
	    {   
         
                ChannelId = channelid; 
                
	    }
	}
}

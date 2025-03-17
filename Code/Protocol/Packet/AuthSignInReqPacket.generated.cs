using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignInReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public string ChannelId { get; set; } = default;
        

        public const string NAME = "auth/sign-in";
        public string GetProtocolName() => NAME;

        public AuthSignInReqPacket( string channelid )
	    {   
         
                ChannelId = channelid; 
                
	    }

    
        public AuthSignInReqPacket()
        {
        }
        

	}
}

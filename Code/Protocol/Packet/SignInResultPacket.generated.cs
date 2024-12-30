using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class SignInResultPacket	
        {
        
                [ProtoMember(1)]
                public string SessionKey { get; set; } 
                
                [ProtoMember(2)]
                public string ChannelKey { get; set; } 
                
                [ProtoMember(3)]
                public string ClientSecret { get; set; } 
                
                [ProtoMember(4)]
                public string AccountEnv { get; set; } 
                
                [ProtoMember(5)]
                public EAccountState AccountState { get; set; } 
                
	}
}

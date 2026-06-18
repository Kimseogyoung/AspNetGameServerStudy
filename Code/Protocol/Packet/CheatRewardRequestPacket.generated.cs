using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CheatRewardRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public List<ObjValue> RewardList { get; set; } = new List<ObjValue>();
        

        public const string NAME = "cheat/reward";
        public string GetProtocolName() => NAME;

        public CheatRewardRequestPacket( List<ObjValue> rewardlist )
	    {   
         
                RewardList = rewardlist; 
                
	    }

    
        public CheatRewardRequestPacket()
        {
        }
        

	}
}

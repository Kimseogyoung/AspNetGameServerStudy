using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CheatRewardReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public List<ObjValue> RewardList { get; set; } = new List<ObjValue>();
        

        public const string NAME = "cheat/reward";
        public string GetProtocolName() => NAME;

        public CheatRewardReqPacket( List<ObjValue> rewardlist )
	    {   
         
                RewardList = rewardlist; 
                
	    }

    
        public CheatRewardReqPacket()
        {
        }
        

	}
}

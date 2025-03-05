using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CheatRewardReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public List<ObjPacket> RewardList { get; set; } 
        
        public string GetProtocolName() => "cheat/reward";

        public CheatRewardReqPacket( ReqInfoPacket info,  List<ObjPacket> rewardlist )
	    {   
         
                Info = info; 
                 
                RewardList = rewardlist; 
                
	    }
	}
}

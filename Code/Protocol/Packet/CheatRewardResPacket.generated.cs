using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CheatRewardResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<ChgObjPacket> ChgObjList { get; set; } 
        
	}
}

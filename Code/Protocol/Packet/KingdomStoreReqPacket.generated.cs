using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStoreReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public List<ulong> KingdomStructureIdList { get; set; } 
        
        [ProtoMember(3)]
        public List<int> KingdomDecoItemNumList { get; set; } 
        
        public string GetProtocolName() => "kingdom/store";
	}
}

using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomFinishConstructStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } = default;
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } = default;
        

        public const string NAME = "kingdom/finish-construct-structure";
        public string GetProtocolName() => NAME;

        public KingdomFinishConstructStructureReqPacket( ulong kingdomstructureid,  int kingdomitemnum )
	    {   
         
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                
	    }

    
        public KingdomFinishConstructStructureReqPacket()
        {
        }
        

	}
}

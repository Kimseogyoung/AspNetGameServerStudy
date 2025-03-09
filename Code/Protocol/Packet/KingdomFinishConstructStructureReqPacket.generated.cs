using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomFinishConstructStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } 
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } 
        

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

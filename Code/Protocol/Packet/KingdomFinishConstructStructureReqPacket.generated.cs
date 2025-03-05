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
        
        public string GetProtocolName() => "kingdom/finish-construct-structure";

        public KingdomFinishConstructStructureReqPacket( ReqInfoPacket info,  ulong kingdomstructureid,  int kingdomitemnum )
	    {   
         
                Info = info; 
                 
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                
	    }
	}
}

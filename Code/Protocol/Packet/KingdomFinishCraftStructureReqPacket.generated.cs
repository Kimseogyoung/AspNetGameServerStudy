using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomFinishCraftStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } 
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } 
        
        public string GetProtocolName() => "kingdom/finish-craft-structure";

        public KingdomFinishCraftStructureReqPacket( ReqInfoPacket info,  ulong kingdomstructureid,  int kingdomitemnum )
	    {   
         
                Info = info; 
                 
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                
	    }
	}
}

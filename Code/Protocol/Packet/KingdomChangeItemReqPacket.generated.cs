using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomChangeItemReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<ulong> StoreKingdomItemIdList { get; set; } = new();
        
        [ProtoMember(3)]
        public List<ChgKingdomItemPacket> ChgKingdomItemList { get; set; } = new();
        
        [ProtoMember(3)]
        public List<ChgKingdomItemPacket> PlaceKingdomItemList { get; set; } = new();
        

        public const string NAME = "kingdom/change-item";
        public string GetProtocolName() => NAME;

        public KingdomChangeItemReqPacket( List<ulong> storekingdomitemidlist,  List<ChgKingdomItemPacket> chgkingdomitemlist,  List<ChgKingdomItemPacket> placekingdomitemlist )
	    {   
         
                StoreKingdomItemIdList = storekingdomitemidlist; 
                 
                ChgKingdomItemList = chgkingdomitemlist; 
                 
                PlaceKingdomItemList = placekingdomitemlist; 
                
	    }

    
        public KingdomChangeItemReqPacket()
        {
        }
        

	}
}

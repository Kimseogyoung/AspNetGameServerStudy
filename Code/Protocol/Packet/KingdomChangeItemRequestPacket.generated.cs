using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomChangeItemRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public List<ulong> StoreKingdomItemIdList { get; set; } = new List<ulong>();
        
        [ProtoMember(3)]
        public List<ChgKingdomItemPacket> ChgKingdomItemList { get; set; } = new List<ChgKingdomItemPacket>();
        
        [ProtoMember(3)]
        public List<ChgKingdomItemPacket> PlaceKingdomItemList { get; set; } = new List<ChgKingdomItemPacket>();
        

        public const string NAME = "kingdom/change-item";
        public string GetProtocolName() => NAME;

        public KingdomChangeItemRequestPacket( List<ulong> storekingdomitemidlist,  List<ChgKingdomItemPacket> chgkingdomitemlist,  List<ChgKingdomItemPacket> placekingdomitemlist )
	    {   
         
                StoreKingdomItemIdList = storekingdomitemidlist; 
                 
                ChgKingdomItemList = chgkingdomitemlist; 
                 
                PlaceKingdomItemList = placekingdomitemlist; 
                
	    }

    
        public KingdomChangeItemRequestPacket()
        {
        }
        

	}
}

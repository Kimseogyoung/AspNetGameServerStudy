using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class PlayerPacket
	{
    
    		[ProtoMember(0)]
    		public ulong Id { get; set; } = default; //
        
    		[ProtoMember(1)]
    		public ulong AccountId { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public ulong SfId { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string ProfileName { get; set; } = ""; //
        
    		[ProtoMember(4)]
    		public int Lv { get; set; } = 1; //
        
    		[ProtoMember(5)]
    		public ulong Flag { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public EPlayerState State { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public int Exp { get; set; } = default; //
        
    		[ProtoMember(8)]
    		public int AccExp { get; set; } = default; //
        
    		[ProtoMember(9)]
    		public int ProfileTitleNum { get; set; } = default; //
        
    		[ProtoMember(10)]
    		public int ProfileIconNum { get; set; } = default; //
        
    		[ProtoMember(11)]
    		public int ProfileFrameNum { get; set; } = default; //
        
    		[ProtoMember(12)]
    		public int ProfileCookieNum { get; set; } = default; //
        
    		[ProtoMember(13)]
    		public ulong GuildId { get; set; } = default; //
        
    		[ProtoMember(14)]
    		public int KingdomExp { get; set; } = default; //
        
    		[ProtoMember(15)]
    		public double Gold { get; set; } = default; //
        
    		[ProtoMember(16)]
    		public double AccGold { get; set; } = default; //
        
    		[ProtoMember(17)]
    		public double RealCash { get; set; } = default; //
        
    		[ProtoMember(18)]
    		public double FreeCash { get; set; } = default; //
        
    		[ProtoMember(19)]
    		public double AccRealCash { get; set; } = default; //
        
    		[ProtoMember(20)]
    		public double AccFreeCash { get; set; } = default; //
        
    		[ProtoMember(21)]
    		public List<KingdomObjPacket> KingdomObjList { get; set; } = default; //
        
    		[ProtoMember(22)]
    		public List<CookiePacket> CookieList { get; set; } = default; //
        
    		[ProtoMember(23)]
    		public List<PointPacket> PointList { get; set; } = default; //
        
    		[ProtoMember(24)]
    		public List<TicketPacket> TicketList { get; set; } = default; //
        
	}
}

using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class PlayerPacket
	{
    
    		[ProtoMember(1)]
    		public ulong SfId { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public string ProfileName { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public int Lv { get; set; } = 1; //
        
    		[ProtoMember(4)]
    		public int CastleLv { get; set; } = 0; //
        
    		[ProtoMember(5)]
    		public ulong Flag { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public EPlayerState State { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public double Exp { get; set; } = default; //
        
    		[ProtoMember(8)]
    		public double AccExp { get; set; } = default; //
        
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
    		public List<CookiePacket> CookieList { get; set; } = new List<CookiePacket>(); //
        
    		[ProtoMember(22)]
    		public List<PointPacket> PointList { get; set; } = new List<PointPacket>(); //
        
    		[ProtoMember(23)]
    		public List<TicketPacket> TicketList { get; set; } = new List<TicketPacket>(); //
        
    		[ProtoMember(24)]
    		public List<ItemPacket> ItemList { get; set; } = new List<ItemPacket>(); //
        
    		[ProtoMember(25)]
    		public List<KingdomStructurePacket> KingdomStructureList { get; set; } = new List<KingdomStructurePacket>(); //
        
    		[ProtoMember(26)]
    		public List<KingdomDecoPacket> KingdomDecoList { get; set; } = new List<KingdomDecoPacket>(); //
        
    		[ProtoMember(27)]
    		public KingdomMapPacket KingdomMap { get; set; } = new KingdomMapPacket(); //
        
    		[ProtoMember(28)]
    		public List<WorldPacket> WorldList { get; set; } = new List<WorldPacket>(); //
        
    		[ProtoMember(29)]
    		public List<WorldStagePacket> WorldStageList { get; set; } = new List<WorldStagePacket>(); //
        
	}
}

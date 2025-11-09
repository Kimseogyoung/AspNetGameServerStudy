using ProtoBuf;
using System.Collections.Generic;
using System;
namespace Proto
{
	[ProtoContract]
	public partial class CookieProto : ProtoBase
	{
    
    		[ProtoMember(2)]
    		public int Num { get; set; }
        
    		[ProtoMember(3)]
    		public string NameKey { get; set; }
        
    		[ProtoMember(4)]
    		public string Name { get; set; }
        
    		[ProtoMember(5)]
    		public EGradeType GradeType { get; set; }
        
    		[ProtoMember(6)]
    		public ECookieRollType RollType { get; set; }
        
    		[ProtoMember(7)]
    		public EFormationPositionType FormationPosType { get; set; }
        
    		[ProtoMember(8)]
    		public int SoulStoneNum { get; set; }
        
    		[ProtoMember(9)]
    		public int InitSoulStone { get; set; }
        
    		[ProtoMember(10)]
    		public int Hp { get; set; }
        
    		[ProtoMember(11)]
    		public int Atk { get; set; }
        
    		[ProtoMember(12)]
    		public int Def { get; set; }
        
    		[ProtoMember(13)]
    		public int Cri { get; set; }
        
    		[ProtoMember(14)]
    		public string Sprite { get; set; }
        
    		[ProtoMember(15)]
    		public string IconSprite { get; set; }
        
	}
}

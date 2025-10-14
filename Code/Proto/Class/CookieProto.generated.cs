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
    		public string Name { get; set; }
        
    		[ProtoMember(4)]
    		public EGradeType GradeType { get; set; }
        
    		[ProtoMember(5)]
    		public ECookieRollType RollType { get; set; }
        
    		[ProtoMember(6)]
    		public EFormationPositionType FormationPosType { get; set; }
        
    		[ProtoMember(7)]
    		public int InitSoulStone { get; set; }
        
    		[ProtoMember(8)]
    		public int Hp { get; set; }
        
    		[ProtoMember(9)]
    		public int Atk { get; set; }
        
    		[ProtoMember(10)]
    		public int Def { get; set; }
        
    		[ProtoMember(11)]
    		public int Cri { get; set; }
        
	}
}

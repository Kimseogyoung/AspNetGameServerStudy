using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class CookieProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public string NameKey { get; set; }
        
    		public string Name { get; set; }
        
    		public EGradeType GradeType { get; set; }
        
    		public ECookieRollType RollType { get; set; }
        
    		public EFormationPositionType FormationPosType { get; set; }
        
    		public int SoulStoneNum { get; set; }
        
    		public int InitSoulStone { get; set; }
        
    		public int Hp { get; set; }
        
    		public int Atk { get; set; }
        
    		public int Def { get; set; }
        
    		public int Cri { get; set; }
        
    		public string Sprite { get; set; }
        
    		public string IconSprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "NameKey": NameKey = value; break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "GradeType": GradeType = Enum.Parse<EGradeType>(value); break;
        
        
        
    				case "RollType": RollType = Enum.Parse<ECookieRollType>(value); break;
        
        
        
    				case "FormationPosType": FormationPosType = Enum.Parse<EFormationPositionType>(value); break;
        
        
        
    				case "SoulStoneNum": SoulStoneNum = int.Parse(value); break;
        
        
        
    				case "InitSoulStone": InitSoulStone = int.Parse(value); break;
        
        
        
    				case "Hp": Hp = int.Parse(value); break;
        
        
        
    				case "Atk": Atk = int.Parse(value); break;
        
        
        
    				case "Def": Def = int.Parse(value); break;
        
        
        
    				case "Cri": Cri = int.Parse(value); break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
        
    				case "IconSprite": IconSprite = value; break;
        
        
			}
		}
	}
}

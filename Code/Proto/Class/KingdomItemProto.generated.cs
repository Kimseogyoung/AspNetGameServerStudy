using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class KingdomItemProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public string Name { get; set; }
        
    		public EKingdomItemType Type { get; set; }
        
    		public EKingdomItemSpecialType SpecialType { get; set; }
        
    		public int SizeX { get; set; }
        
    		public int SizeY { get; set; }
        
    		public string Sprite { get; set; }
        
    		public int MaxCnt { get; set; }
        
    		public int MaxLv { get; set; }
        
    		public int CastleLv { get; set; }
        
    		public int ConstructSec { get; set; }
        
    		public EObjType ConstructObjType { get; set; }
        
    		public int ConstructObjNum { get; set; }
        
    		public int ConstructObjAmount { get; set; }
        
    		public EObjType CostObjType { get; set; }
        
    		public int CostObjNum { get; set; }
        
    		public int CostObjAmount { get; set; }
        
    		public int ProductionSec { get; set; }
        
    		public EObjType ProductObjType { get; set; }
        
    		public int ProductObjNum { get; set; }
        
    		public int ProductObjAmount { get; set; }
        
    		public int DecoPoint { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "Type": Type = Enum.Parse<EKingdomItemType>(value); break;
        
        
        
    				case "SpecialType": SpecialType = Enum.Parse<EKingdomItemSpecialType>(value); break;
        
        
        
    				case "SizeX": SizeX = int.Parse(value); break;
        
        
        
    				case "SizeY": SizeY = int.Parse(value); break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
        
    				case "MaxCnt": MaxCnt = int.Parse(value); break;
        
        
        
    				case "MaxLv": MaxLv = int.Parse(value); break;
        
        
        
    				case "CastleLv": CastleLv = int.Parse(value); break;
        
        
        
    				case "ConstructSec": ConstructSec = int.Parse(value); break;
        
        
        
    				case "ConstructObjType": ConstructObjType = Enum.Parse<EObjType>(value); break;
        
        
        
    				case "ConstructObjNum": ConstructObjNum = int.Parse(value); break;
        
        
        
    				case "ConstructObjAmount": ConstructObjAmount = int.Parse(value); break;
        
        
        
    				case "CostObjType": CostObjType = Enum.Parse<EObjType>(value); break;
        
        
        
    				case "CostObjNum": CostObjNum = int.Parse(value); break;
        
        
        
    				case "CostObjAmount": CostObjAmount = int.Parse(value); break;
        
        
        
    				case "ProductionSec": ProductionSec = int.Parse(value); break;
        
        
        
    				case "ProductObjType": ProductObjType = Enum.Parse<EObjType>(value); break;
        
        
        
    				case "ProductObjNum": ProductObjNum = int.Parse(value); break;
        
        
        
    				case "ProductObjAmount": ProductObjAmount = int.Parse(value); break;
        
        
        
    				case "DecoPoint": DecoPoint = int.Parse(value); break;
        
        
			}
		}
	}
}

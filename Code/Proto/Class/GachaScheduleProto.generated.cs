using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class GachaScheduleProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public string Tag { get; set; }
        
    		public int DisplayOrder { get; set; }
        
    		public int Seq { get; set; }
        
    		public string NameKey { get; set; }
        
    		public string Name { get; set; }
        
    		public int GachaProbNum { get; set; }
        
    		public List<int> PickupCookieNumList { get; set; }
        
    		public List<EObjType> CostTypeList { get; set; }
        
    		public List<int> CostAmountList { get; set; }
        
    		public List<int> CntList { get; set; }
        
    		public string BGSprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Tag": Tag = value; break;
        
        
        
    				case "DisplayOrder": DisplayOrder = int.Parse(value); break;
        
        
        
    				case "Seq": Seq = int.Parse(value); break;
        
        
        
    				case "NameKey": NameKey = value; break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "GachaProbNum": GachaProbNum = int.Parse(value); break;
        
        
        
    				case "PickupCookieNumList":
    					if (PickupCookieNumList == null) PickupCookieNumList = new List<int>();
    					PickupCookieNumList.Add(int.Parse(value)); break;
        
        
        
    				case "CostTypeList":
    					if (CostTypeList == null) CostTypeList = new List<EObjType>();
    					CostTypeList.Add(Enum.Parse<EObjType>(value)); break;
        
        
        
    				case "CostAmountList":
    					if (CostAmountList == null) CostAmountList = new List<int>();
    					CostAmountList.Add(int.Parse(value)); break;
        
        
        
    				case "CntList":
    					if (CntList == null) CntList = new List<int>();
    					CntList.Add(int.Parse(value)); break;
        
        
        
    				case "BGSprite": BGSprite = value; break;
        
        
			}
		}
	}
}

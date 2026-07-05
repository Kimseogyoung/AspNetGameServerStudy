using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class WorldStageProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public int WorldNum { get; set; }
        
    		public int Order { get; set; }
        
    		public int Lv { get; set; }
        
    		public EWorldStageType Type { get; set; }
        
    		public string Name { get; set; }
        
    		public int BossNum { get; set; }
        
    		public int SteminaCnt { get; set; }
        
    		public List<EObjType> FirstRewardTypeList { get; set; }
        
    		public List<int> FirstRewardNumList { get; set; }
        
    		public List<int> FirstRewardAmountList { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "WorldNum": WorldNum = int.Parse(value); break;
        
        
        
    				case "Order": Order = int.Parse(value); break;
        
        
        
    				case "Lv": Lv = int.Parse(value); break;
        
        
        
    				case "Type": Type = Enum.Parse<EWorldStageType>(value); break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "BossNum": BossNum = int.Parse(value); break;
        
        
        
    				case "SteminaCnt": SteminaCnt = int.Parse(value); break;
        
        
        
    				case "FirstRewardTypeList":
    					if (FirstRewardTypeList == null) FirstRewardTypeList = new List<EObjType>();
    					FirstRewardTypeList.Add(Enum.Parse<EObjType>(value)); break;
        
        
        
    				case "FirstRewardNumList":
    					if (FirstRewardNumList == null) FirstRewardNumList = new List<int>();
    					FirstRewardNumList.Add(int.Parse(value)); break;
        
        
        
    				case "FirstRewardAmountList":
    					if (FirstRewardAmountList == null) FirstRewardAmountList = new List<int>();
    					FirstRewardAmountList.Add(int.Parse(value)); break;
        
        
			}
		}
	}
}

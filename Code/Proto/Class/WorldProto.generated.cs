using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class WorldProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public EWorldType Type { get; set; }
        
    		public int Order { get; set; }
        
    		public string Name { get; set; }
        
    		public List<int> RewardStarList { get; set; }
        
    		public List<int> RewardStarCashList { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Type": Type = Enum.Parse<EWorldType>(value); break;
        
        
        
    				case "Order": Order = int.Parse(value); break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "RewardStarList":
    					if (RewardStarList == null) RewardStarList = new List<int>();
    					RewardStarList.Add(int.Parse(value)); break;
        
        
        
    				case "RewardStarCashList":
    					if (RewardStarCashList == null) RewardStarCashList = new List<int>();
    					RewardStarCashList.Add(int.Parse(value)); break;
        
        
			}
		}
	}
}

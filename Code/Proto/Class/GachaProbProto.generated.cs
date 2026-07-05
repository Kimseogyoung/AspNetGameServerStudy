using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class GachaProbProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public List<int> GradeWeightList { get; set; }
        
    		public List<int> PickupWeightList { get; set; }
        
    		public int WeightSum { get; set; }
        
    		public List<int> DetailWeightList { get; set; }
        
    		public int DetailWeightSum { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "GradeWeightList":
    					if (GradeWeightList == null) GradeWeightList = new List<int>();
    					GradeWeightList.Add(int.Parse(value)); break;
        
        
        
    				case "PickupWeightList":
    					if (PickupWeightList == null) PickupWeightList = new List<int>();
    					PickupWeightList.Add(int.Parse(value)); break;
        
        
        
    				case "WeightSum": WeightSum = int.Parse(value); break;
        
        
        
    				case "DetailWeightList":
    					if (DetailWeightList == null) DetailWeightList = new List<int>();
    					DetailWeightList.Add(int.Parse(value)); break;
        
        
        
    				case "DetailWeightSum": DetailWeightSum = int.Parse(value); break;
        
        
			}
		}
	}
}

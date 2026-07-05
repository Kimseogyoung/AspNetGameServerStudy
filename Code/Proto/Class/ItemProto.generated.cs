using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class ItemProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public EItemType Type { get; set; }
        
    		public string NameKey { get; set; }
        
    		public string Name { get; set; }
        
    		public int DisplayOrder { get; set; }
        
    		public EObjType SaleObjType { get; set; }
        
    		public int SaleObjNum { get; set; }
        
    		public int SaleObjAmount { get; set; }
        
    		public string Sprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Type": Type = Enum.Parse<EItemType>(value); break;
        
        
        
    				case "NameKey": NameKey = value; break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "DisplayOrder": DisplayOrder = int.Parse(value); break;
        
        
        
    				case "SaleObjType": SaleObjType = Enum.Parse<EObjType>(value); break;
        
        
        
    				case "SaleObjNum": SaleObjNum = int.Parse(value); break;
        
        
        
    				case "SaleObjAmount": SaleObjAmount = int.Parse(value); break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
			}
		}
	}
}

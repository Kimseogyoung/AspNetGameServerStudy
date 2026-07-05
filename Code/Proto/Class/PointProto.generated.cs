using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class PointProto : ProtoBase
	{
    
    		public EObjType Type { get; set; }
        
    		public string NameKey { get; set; }
        
    		public string Name { get; set; }
        
    		public string Sprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Type": Type = Enum.Parse<EObjType>(value); break;
        
        
        
    				case "NameKey": NameKey = value; break;
        
        
        
    				case "Name": Name = value; break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
			}
		}
	}
}

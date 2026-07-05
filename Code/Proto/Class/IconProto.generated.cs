using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class IconProto : ProtoBase
	{
    
    		public string Key { get; set; }
        
    		public string Sprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Key": Key = value; break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
			}
		}
	}
}

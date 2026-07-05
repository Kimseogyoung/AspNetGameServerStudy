using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class CookieSoulStoneProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public int CookieNum { get; set; }
        
    		public string Key { get; set; }
        
    		public string Sprite { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "CookieNum": CookieNum = int.Parse(value); break;
        
        
        
    				case "Key": Key = value; break;
        
        
        
    				case "Sprite": Sprite = value; break;
        
        
			}
		}
	}
}

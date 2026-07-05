using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class LocalizationProto : ProtoBase
	{
    
    		public string Key { get; set; }
        
    		public string ko { get; set; }
        
    		public string en { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Key": Key = value; break;
        
        
        
    				case "ko": ko = value; break;
        
        
        
    				case "en": en = value; break;
        
        
			}
		}
	}
}

using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class CookieStarEnhanceProto : ProtoBase
	{
    
    		public EGradeType Type { get; set; }
        
    		public int Star { get; set; }
        
    		public int SoulStone { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Type": Type = Enum.Parse<EGradeType>(value); break;
        
        
        
    				case "Star": Star = int.Parse(value); break;
        
        
        
    				case "SoulStone": SoulStone = int.Parse(value); break;
        
        
			}
		}
	}
}

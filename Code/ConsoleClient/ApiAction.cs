using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore
{
    public class ApiFunc
    {
        public string ApiPath { get; set; }
        public string Desc { get; set; }
        public Func<string[], Task> Action { get; set; }
    }

}

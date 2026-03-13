using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public interface IReqPacket
    {
        public ReqInfoPacket Info { get; set; }
        public string GetProtocolName();
    }

    public interface IResPacket
    {
        public ResInfoPacket Info { get; set; }
    }
}

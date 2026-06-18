using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public interface IRequestPacket
    {
        public RequestInfoPacket Info { get; set; }
        public string GetProtocolName();
    }

    public interface IResponsePacket
    {
        public ResponseInfoPacket Info { get; set; }
    }
}

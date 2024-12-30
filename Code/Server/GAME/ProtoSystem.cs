using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WebStudyServer.StartUp;
using Microsoft.VisualBasic;
using System.Diagnostics;
using Protocol;
using Proto;
using WebStudyServer.GAME;

namespace WebStudyServer
{ 
    public class ProtoSystem
    {

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            var csvPath = Path.GetFullPath(config.GetValue("Proto:CsvPath", ""));
            _prt.Init(csvPath);
        }

        public void Bind()
        {
            _prt.Bind<KingdomItemProto>();
            _prt.Bind<PointProto>();
            _prt.Bind<TicketProto>();
            _prt.Bind<CookieProto>();
        }

        public CookieProto GetCookiePrt(int cookieNum) => _prt.Get<CookieProto>(cookieNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => _prt.Get<KingdomItemProto>(kingdomObjNum);
        public PointProto GetPointPrt(EObjType objType) => _prt.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => _prt.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);

        private static ProtoHelper _prt = new ProtoHelper();
    }
}

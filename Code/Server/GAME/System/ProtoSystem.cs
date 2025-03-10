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
            _prt.Bind<ItemProto>();
            _prt.Bind<PointProto>();
            _prt.Bind<TicketProto>();
            _prt.Bind<CookieProto>();
            _prt.Bind<CookieStarEnhanceProto>();
            _prt.Bind<ScheduleProto>();
            _prt.Bind<GachaScheduleProto>();
            _prt.Bind<GachaProbProto>();
            _prt.Bind<GachaItemProto>();
            _prt.Bind<CookieSoulStoneProto>();
            _prt.Bind<WorldProto>();
            _prt.Bind<WorldStageProto>();
        }

        // PK
        public CookieProto GetCookiePrt(int cookieNum) => _prt.Get<CookieProto>(cookieNum);
        public CookieStarEnhanceProto GetCookieStarEnhancePrt(EGradeType gradeType, int star) => _prt.Get<CookieStarEnhanceProto>((gradeType, star));
        public CookieSoulStoneProto GetCookieSoulStonePrt(int soulStoneNum) => _prt.Get<CookieSoulStoneProto>(soulStoneNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => _prt.Get<KingdomItemProto>(kingdomObjNum);
        public ItemProto GetItemPrt(int itemNum) => _prt.Get<ItemProto>(itemNum);
        public PointProto GetPointPrt(EObjType objType) => _prt.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => _prt.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);
        public ScheduleProto GetSchedulePrt(int scheduleNum) => _prt.Get<ScheduleProto>(scheduleNum);
        public GachaScheduleProto GetGachaSchedulePrt(int scheduleNum) => _prt.Get<GachaScheduleProto>(scheduleNum);
        public GachaProbProto GetGachaProbPrt(int gachaProbNum) => _prt.Get<GachaProbProto>(gachaProbNum);
        public WorldProto GetWorldPrt(int worldNum) => _prt.Get<WorldProto>(worldNum);
        public WorldStageProto GetWorldStagePrt(int worldStageNum) => _prt.Get<WorldStageProto>(worldStageNum);


        // ALL
        public IEnumerable<ScheduleProto> GetSchedulePrts() => _prt.GetAll<ScheduleProto>();
        public IEnumerable<GachaScheduleProto> GetGachaSchedulePrts() => _prt.GetAll<GachaScheduleProto>();
        public IEnumerable<GachaProbProto> GetGachaProbPrts() => _prt.GetAll<GachaProbProto>();
        public IEnumerable<CookieSoulStoneProto> GetCookieSoulStonePrts() => _prt.GetAll<CookieSoulStoneProto>();
        public IEnumerable<GachaItemProto> GetGachaItemPrts() => _prt.GetAll<GachaItemProto>();
        public IEnumerable<CookieProto> GetCookiePrts() => _prt.GetAll<CookieProto>();
        private static ProtoHelper _prt = new ProtoHelper();
    }
}

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Proto;
using Protocol;
using WebStudyServer.GAME;
using WebStudyServer.StartUp;

namespace WebStudyServer
{
    public class ProtoSystem
    {

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            var csvPath = Path.GetFullPath(config.GetValue("Proto:CsvPath", ""));
            Prt.Init(csvPath);
        }

        public void Bind()
        {
            Prt.Bind<KingdomItemProto>();
            Prt.Bind<ItemProto>();
            Prt.Bind<PointProto>();
            Prt.Bind<TicketProto>();
            Prt.Bind<CookieProto>();
            Prt.Bind<CookieStarEnhanceProto>();
            Prt.Bind<ScheduleProto>();
            Prt.Bind<GachaScheduleProto>();
            Prt.Bind<GachaProbProto>();
            Prt.Bind<GachaItemProto>();
            Prt.Bind<CookieSoulStoneProto>();
            Prt.Bind<WorldProto>();
            Prt.Bind<WorldStageProto>();
        }

        // PK
        public CookieProto GetCookiePrt(int cookieNum) => Prt.Get<CookieProto>(cookieNum);
        public CookieStarEnhanceProto GetCookieStarEnhancePrt(EGradeType gradeType, int star) => Prt.Get<CookieStarEnhanceProto>((gradeType, star));
        public CookieSoulStoneProto GetCookieSoulStonePrt(int soulStoneNum) => Prt.Get<CookieSoulStoneProto>(soulStoneNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => Prt.Get<KingdomItemProto>(kingdomObjNum);
        public ItemProto GetItemPrt(int itemNum) => Prt.Get<ItemProto>(itemNum);
        public PointProto GetPointPrt(EObjType objType) => Prt.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => Prt.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);
        public ScheduleProto GetSchedulePrt(int scheduleNum) => Prt.Get<ScheduleProto>(scheduleNum);
        public GachaScheduleProto GetGachaSchedulePrt(int scheduleNum) => Prt.Get<GachaScheduleProto>(scheduleNum);
        public GachaProbProto GetGachaProbPrt(int gachaProbNum) => Prt.Get<GachaProbProto>(gachaProbNum);
        public WorldProto GetWorldPrt(int worldNum) => Prt.Get<WorldProto>(worldNum);
        public WorldStageProto GetWorldStagePrt(int worldStageNum) => Prt.Get<WorldStageProto>(worldStageNum);

        // MK
        public List<WorldProto> GetWorldPrtListByMk(EWorldType type) => Prt.GetByMk<WorldProto>(type);
        public List<WorldStageProto> GetWorldStagePrtListByMk(int worldNum) => Prt.GetByMk<WorldStageProto>(worldNum);

        // ALL
        public IEnumerable<ScheduleProto> GetSchedulePrts() => Prt.GetAll<ScheduleProto>();
        public IEnumerable<GachaScheduleProto> GetGachaSchedulePrts() => Prt.GetAll<GachaScheduleProto>();
        public IEnumerable<GachaProbProto> GetGachaProbPrts() => Prt.GetAll<GachaProbProto>();
        public IEnumerable<CookieSoulStoneProto> GetCookieSoulStonePrts() => Prt.GetAll<CookieSoulStoneProto>();
        public IEnumerable<GachaItemProto> GetGachaItemPrts() => Prt.GetAll<GachaItemProto>();
        public IEnumerable<CookieProto> GetCookiePrts() => Prt.GetAll<CookieProto>();
        private static readonly ProtoHelper Prt = new();



    }
}

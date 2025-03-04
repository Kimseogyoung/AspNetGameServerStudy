using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Proto;
namespace Client
{
    public class ProtoSystem
    {
        public void Init(string inCsvPath)
        {
            var csvPath = Path.GetFullPath(inCsvPath);
            _prt.Init(csvPath);
        }

        public void Bind()
        {
            _prt.Bind<KingdomItemProto>();
            _prt.Bind<ItemProto>();
            _prt.Bind<PointProto>();
            _prt.Bind<TicketProto>();
            _prt.Bind<CookieProto>();
            _prt.Bind<CookieSoulStoneProto>();
            _prt.Bind<CookieStarEnhanceProto>();
            _prt.Bind<ScheduleProto>();
            _prt.Bind<GachaScheduleProto>();
            _prt.Bind<GachaProbProto>();
        }

        public ScheduleProto GetSchedulePrt(int scheduleNum) => _prt.Get<ScheduleProto>(scheduleNum);
        public GachaScheduleProto GetGachaSchedulePrt(int scheduleNum) => _prt.Get<GachaScheduleProto>(scheduleNum);
        public CookieProto GetCookiePrt(int cookieNum) => _prt.Get<CookieProto>(cookieNum);
        public CookieSoulStoneProto GetCookieSoulStonePrt(int soulStoneNum) => _prt.Get<CookieSoulStoneProto>(soulStoneNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => _prt.Get<KingdomItemProto>(kingdomObjNum);
        public ItemProto GetItemPrt(int itemNum) => _prt.Get<ItemProto>(itemNum);
        public PointProto GetPointPrt(EObjType objType) => _prt.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => _prt.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);

        public IEnumerable<CookieProto> GetCookiePrts() => _prt.GetAll<CookieProto>();
        public IEnumerable<KingdomItemProto> GetKingdomItemPrts() => _prt.GetAll<KingdomItemProto>();
        public IEnumerable<ItemProto> GetItemPrts() => _prt.GetAll<ItemProto>();
        public IEnumerable<PointProto> GetPointPrts() => _prt.GetAll<PointProto>();
        public IEnumerable<TicketProto> GetTicketPrts() => _prt.GetAll<TicketProto>();

        private static ProtoHelper _prt = new ProtoHelper();
    }
}

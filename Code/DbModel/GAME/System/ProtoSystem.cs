using System.Diagnostics;
using Proto;

namespace WebStudyServer
{
    public class ProtoSystem
    {

        public void Init(IConfiguration config, IHostEnvironment environ)
        {
            var csvPath = Path.GetFullPath(config.GetValue("Proto:CsvPath", ""));
            ProtoDb.Initialize(new CsvProtoLoader(csvPath));
        }

        public Task<IReadOnlyList<ValidateResult>> LoadAsync()
        {
            var builder = ProtoDb.CreateBuilder()
                .Add(new ParallelLoadDescriptor<KingdomItemProto>())
                .Add(new ParallelLoadDescriptor<ItemProto>())
                .Add(new ParallelLoadDescriptor<PointProto>())
                .Add(new ParallelLoadDescriptor<TicketProto>())
                .Add(new ParallelLoadDescriptor<CookieProto>())
                .Add(new ParallelLoadDescriptor<CookieSoulStoneProto>())
                .Add(new ParallelLoadDescriptor<CookieStarEnhanceProto>())
                .Add(new ParallelLoadDescriptor<ScheduleProto>())
                .Add(new ParallelLoadDescriptor<WorldProto>())
                .Add(new ParallelLoadDescriptor<WorldStageProto>())
                .Add(new ParallelLoadDescriptor<LocalizationProto>())
                .Add(new ParallelLoadDescriptor<GachaItemProto>())
                .Add(new OrderedLoadDescriptor(
                    new ParallelLoadDescriptor<GachaScheduleProto>(),
                    new ParallelLoadDescriptor<GachaProbProto>()
                ));
            return builder.LoadAllAsync();
        }


        // PK
        public CookieProto GetCookiePrt(int cookieNum) => ProtoDb.Get<CookieProto>(cookieNum);
        public CookieStarEnhanceProto GetCookieStarEnhancePrt(EGradeType gradeType, int star) => ProtoDb.Get<CookieStarEnhanceProto>((gradeType, star));
        public CookieSoulStoneProto GetCookieSoulStonePrt(int soulStoneNum) => ProtoDb.Get<CookieSoulStoneProto>(soulStoneNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => ProtoDb.Get<KingdomItemProto>(kingdomObjNum);
        public ItemProto GetItemPrt(int itemNum) => ProtoDb.Get<ItemProto>(itemNum);
        public PointProto GetPointPrt(EObjType objType) => ProtoDb.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => ProtoDb.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);
        public ScheduleProto GetSchedulePrt(int scheduleNum) => ProtoDb.Get<ScheduleProto>(scheduleNum);
        public GachaScheduleProto GetGachaSchedulePrt(int scheduleNum) => ProtoDb.Get<GachaScheduleProto>(scheduleNum);
        public GachaProbProto GetGachaProbPrt(int gachaProbNum) => ProtoDb.Get<GachaProbProto>(gachaProbNum);
        public WorldProto GetWorldPrt(int worldNum) => ProtoDb.Get<WorldProto>(worldNum);
        public WorldStageProto GetWorldStagePrt(int worldStageNum) => ProtoDb.Get<WorldStageProto>(worldStageNum);

        // MK
        public List<WorldProto> GetWorldPrtListByMk(EWorldType type) => ProtoDb.GetByMk<WorldProto>(type);
        public List<WorldStageProto> GetWorldStagePrtListByMk(int worldNum) => ProtoDb.GetByMk<WorldStageProto>(worldNum);

        // ALL
        public IEnumerable<ScheduleProto> GetSchedulePrts() => ProtoDb.GetAll<ScheduleProto>();
        public IEnumerable<GachaScheduleProto> GetGachaSchedulePrts() => ProtoDb.GetAll<GachaScheduleProto>();
        public IEnumerable<GachaProbProto> GetGachaProbPrts() => ProtoDb.GetAll<GachaProbProto>();
        public IEnumerable<CookieSoulStoneProto> GetCookieSoulStonePrts() => ProtoDb.GetAll<CookieSoulStoneProto>();
        public IEnumerable<GachaItemProto> GetGachaItemPrts() => ProtoDb.GetAll<GachaItemProto>();
        public IEnumerable<CookieProto> GetCookiePrts() => ProtoDb.GetAll<CookieProto>();
    }
}

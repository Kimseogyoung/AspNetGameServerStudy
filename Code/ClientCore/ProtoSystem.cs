using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Proto;

namespace ClientCore
{
    public class ProtoSystem
    {
        public void Init(string inCsvPath)
        {
            var csvPath = Path.GetFullPath(inCsvPath);
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
        public ScheduleProto GetSchedulePrt(int scheduleNum) => ProtoDb.Get<ScheduleProto>(scheduleNum);
        public GachaScheduleProto GetGachaSchedulePrt(int scheduleNum) => ProtoDb.Get<GachaScheduleProto>(scheduleNum);
        public CookieProto GetCookiePrt(int cookieNum) => ProtoDb.Get<CookieProto>(cookieNum);
        public CookieStarEnhanceProto GetCookieStarEnhancePrt(EGradeType gradeType, int star) => ProtoDb.Get<CookieStarEnhanceProto>((gradeType, star));
        public CookieSoulStoneProto GetCookieSoulStonePrt(int soulStoneNum) => ProtoDb.Get<CookieSoulStoneProto>(soulStoneNum);
        public KingdomItemProto GetKingdomItemPrt(int kingdomObjNum) => ProtoDb.Get<KingdomItemProto>(kingdomObjNum);
        public ItemProto GetItemPrt(int itemNum) => ProtoDb.Get<ItemProto>(itemNum);
        public PointProto GetPointPrt(EObjType objType) => ProtoDb.Get<PointProto>(objType);
        public PointProto GetPointPrt(int pointNum) => GetPointPrt((EObjType)pointNum);
        public TicketProto GetTicketPrt(EObjType objType) => ProtoDb.Get<TicketProto>(objType);
        public TicketProto GetTicketPrt(int ticketNum) => GetTicketPrt((EObjType)ticketNum);
        public WorldProto GetWorldPrt(int worldNum) => ProtoDb.Get<WorldProto>(worldNum);
        public WorldStageProto GetWorldStagePrt(int worldStageNum) => ProtoDb.Get<WorldStageProto>(worldStageNum);
        public LocalizationProto GetLocalizationPrt(string key) => ProtoDb.Get<LocalizationProto>(key);
        public bool TryGetLocalizationPrt(string key, out LocalizationProto? prt) => ProtoDb.TryGet(key, out prt);

        // MK
        public List<WorldStageProto> GetWorldStagePrtListByMk(int worldNum) => ProtoDb.GetByMk<WorldStageProto>(worldNum);

        // All
        public IEnumerable<CookieProto> GetCookiePrts() => ProtoDb.GetAll<CookieProto>();
        public IEnumerable<KingdomItemProto> GetKingdomItemPrts() => ProtoDb.GetAll<KingdomItemProto>();
        public IEnumerable<ItemProto> GetItemPrts() => ProtoDb.GetAll<ItemProto>();
        public IEnumerable<PointProto> GetPointPrts() => ProtoDb.GetAll<PointProto>();
        public IEnumerable<TicketProto> GetTicketPrts() => ProtoDb.GetAll<TicketProto>();
        public IEnumerable<LocalizationProto> GetLocalizationPrts() => ProtoDb.GetAll<LocalizationProto>();
    }
}

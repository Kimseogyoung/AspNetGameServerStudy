using Proto;
using Server.Helper;

namespace WebStudyServer
{
    public partial class Startup
    {
        public async Task ProtoAsync(IServiceCollection services)
        {
            var csvPath = Path.GetFullPath(Configuration.GetValue("Proto:CsvPath", ""));
            ProtoDb.Initialize(new CsvProtoLoader(csvPath));

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
            await builder.LoadAllAsync();

            GachaConstant.Init([.. ProtoDb.GetAll<ScheduleProto>()], [.. ProtoDb.GetAll<GachaScheduleProto>()],
                [.. ProtoDb.GetAll<GachaProbProto>()], [.. ProtoDb.GetAll<GachaItemProto>()],
                [.. ProtoDb.GetAll<CookieProto>()], [.. ProtoDb.GetAll<CookieSoulStoneProto>()]);
        }
    }
}

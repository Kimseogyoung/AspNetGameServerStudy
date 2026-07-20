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
    }
}

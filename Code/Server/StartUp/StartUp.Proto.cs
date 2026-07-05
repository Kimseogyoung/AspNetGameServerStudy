using Server.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer
{
    public partial class Startup
    {
        public async Task ProtoAsync(IServiceCollection services)
        {
            await APP.Prt.LoadAsync();

            GachaConstant.Init([.. APP.Prt.GetSchedulePrts()], [.. APP.Prt.GetGachaSchedulePrts()],
                [.. APP.Prt.GetGachaProbPrts()], [.. APP.Prt.GetGachaItemPrts()],
                [.. APP.Prt.GetCookiePrts()], [.. APP.Prt.GetCookieSoulStonePrts()]);
        }
    }
}

using System.Net;
using Microsoft.OpenApi.Models;
using Proto;
using Server.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Proto(IServiceCollection services)
        {
            APP.Prt.Bind();

            GachaConstant.Init([.. APP.Prt.GetSchedulePrts()], [.. APP.Prt.GetGachaSchedulePrts()],
                [.. APP.Prt.GetGachaProbPrts()], [.. APP.Prt.GetGachaItemPrts()],
                [.. APP.Prt.GetCookiePrts()], [.. APP.Prt.GetCookieSoulStonePrts()]);
        }
    }
}

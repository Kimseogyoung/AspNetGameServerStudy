using Microsoft.OpenApi.Models;
using Proto;
using Server.Helper;
using System.Net;
using WebStudyServer.GAME;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Proto(IServiceCollection services)
        {
            APP.Prt.Bind();

            GachaConstant.Init(APP.Prt.GetSchedulePrts().ToList(), APP.Prt.GetGachaSchedulePrts().ToList(), 
                APP.Prt.GetGachaProbPrts().ToList(), APP.Prt.GetGachaItemPrts().ToList(), 
                APP.Prt.GetCookiePrts().ToList(), APP.Prt.GetCookieSoulStonePrts().ToList());
        }
    }
}

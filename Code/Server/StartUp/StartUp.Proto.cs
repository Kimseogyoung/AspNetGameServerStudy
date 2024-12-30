using Microsoft.OpenApi.Models;
using Proto;
using System.Net;
using WebStudyServer.GAME;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Proto(IServiceCollection services)
        {
            APP.PRT.Bind<KingdomObjProto>();
            APP.PRT.Bind<PointProto>();
            APP.PRT.Bind<TicketProto>();
            APP.PRT.Bind<CookieProto>();
        
            var prt = APP.PRT.Get<PointProto>(EObjType.POINT_MILEAGE);
        }
    }
}

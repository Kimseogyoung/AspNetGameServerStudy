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
            APP.Prt.Bind();
        }
    }
}

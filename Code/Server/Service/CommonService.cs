using Protocol;
using WebStudyServer;
using WebStudyServer.Data;
namespace WebStudyServer.Service
{
    public class CommonService : ServiceBase
    {
        public CommonService(GameDb db, RpcContext rpcContext, ILogger<CommonService> logger) : base(db, rpcContext, logger)
        {
        }

        public HealthCheckResponsePacket HealthCheck()
        {
            return new HealthCheckResponsePacket
            {
                Msg = "OK"
            };
        }
    }
}

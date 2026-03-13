using Protocol;
using WebStudyServer;
using WebStudyServer.Base;
using WebStudyServer.Repo;
namespace WebStudyServer.Service
{
    public class CommonService : ServiceBase
    {
        public CommonService(RpcContext rpcContext, ILogger<CommonService> logger) : base(rpcContext, logger)
        {
        }

        public HealthCheckResPacket HealthCheck()
        {
            return new HealthCheckResPacket
            {
                Msg = "OK"
            };
        }
    }
}

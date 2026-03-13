
namespace WebStudyServer
{
    public class ServiceBase
    {
        protected RpcContext RpcContext { get; private set; }
        protected ILogger Logger { get; private set; }
        public ServiceBase(RpcContext rpcContext, ILogger logger)
        {
            RpcContext = rpcContext;
            Logger = logger;
        }
    }
}

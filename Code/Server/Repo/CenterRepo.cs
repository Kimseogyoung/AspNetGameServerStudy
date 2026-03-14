using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

namespace WebStudyServer.Repo
{
    public class CenterRepo : RepoBase
    {
        public ScheduleComponent Schedule { get; private set; }
        public RpcContext RpcContext { get; private set; }

        public CenterRepo(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Schedule = new ScheduleComponent(this, _dbFactory);
        }

        public static CenterRepo CreateInstance(RpcContext rpcContext)
        {
            var centerRepo = new CenterRepo(rpcContext);
            return centerRepo;
        }
    }
}

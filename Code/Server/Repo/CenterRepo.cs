using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

namespace WebStudyServer.Repo
{
    public class CenterRepo : RepoBase
    {
        public ScheduleComponent Schedule => _scheduleComponent;
        public RpcContext RpcContext { get; private set; }

        public CenterRepo(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            _scheduleComponent = new ScheduleComponent(this, _executor);
        }

        public static CenterRepo CreateInstance(RpcContext rpcContext)
        {
            var centerRepo = new CenterRepo(rpcContext);
            return centerRepo;
        }

        private ScheduleComponent _scheduleComponent;
    }
}

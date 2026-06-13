using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

namespace WebStudyServer.Repo
{
    public class CenterRepo : RepoBase
    {
        public ScheduleComponent Schedule { get; private set; }
        public RpcContext RpcContext { get; private set; }

        public CenterRepo(RpcContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Schedule = new ScheduleComponent(this, Repository);
        }
    }
}

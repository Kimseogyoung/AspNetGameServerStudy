using ServerCore;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;

namespace WebStudyServer.Repo
{
    public class CenterRepo : RepoBase
    {
        public ScheduleComponent Schedule { get; private set; }
        public IGameContext RpcContext { get; private set; }

        public CenterRepo(IGameContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
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

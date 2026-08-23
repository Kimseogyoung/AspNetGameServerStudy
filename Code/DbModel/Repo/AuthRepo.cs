using ServerCore;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using ServerCore.Extension;
using WebStudyServer.Model;

namespace WebStudyServer.Repo
{
    public class AuthRepo : RepoBase
    {
        public PlayerMapComponent PlayerMap { get; private set; }

        public IGameContext RpcContext { get; private set; }
        public AuthRepo(IGameContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            PlayerMap = new PlayerMapComponent(this, Repository);
        }

    }
}

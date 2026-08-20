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
        public SessionComponent Session { get; private set; }
        public PlayerMapComponent PlayerMap { get; private set; }

        public IGameContext RpcContext { get; private set; }
        public AuthRepo(IGameContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Session = new SessionComponent(this, Repository);
            PlayerMap = new PlayerMapComponent(this, Repository);
        }

    }
}

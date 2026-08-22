using ServerCore;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;

namespace WebStudyServer.Repo
{
    public class UserRepo : RepoBase
    {
        public PlayerComponent Player { get; private set; }
        public PlayerDetailComponent PlayerDetail { get; private set; }
        public KingdomStructureComponent KingdomStructure { get; private set; }
        public KingdomDecoComponent KingdomDeco { get; private set; }
        public KingdomMapComponent KingdomMap { get; private set; }
        public WorldComponent World { get; private set; }
        public WorldStageComponent WorldStage { get; private set; }
        public IGameContext RpcContext { get; private set; }

        public UserRepo(IGameContext rpcContext, IRepository repository): base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Player = new PlayerComponent(this, Repository);
            PlayerDetail = new PlayerDetailComponent(this, Repository);
            KingdomStructure = new KingdomStructureComponent(this, Repository);
            KingdomDeco = new KingdomDecoComponent(this, Repository);
            KingdomMap = new KingdomMapComponent(this, Repository);
            World = new WorldComponent(this, Repository);
            WorldStage = new WorldStageComponent(this, Repository);
        }
    }
}

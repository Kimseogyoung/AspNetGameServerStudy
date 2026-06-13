using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

namespace WebStudyServer.Repo
{
    public class UserRepo : RepoBase
    {
        public PlayerComponent Player { get; private set; }
        public PlayerDetailComponent PlayerDetail { get; private set; }
        public PointComponent Point { get; private set; }
        public TicketComponent Ticket { get; private set; }
        public CookieComponent Cookie { get; private set; }
        public ItemComponent Item { get; private set; }
        public KingdomStructureComponent KingdomStructure { get; private set; }
        public KingdomDecoComponent KingdomDeco { get; private set; }
        public KingdomMapComponent KingdomMap { get; private set; }
        public WorldComponent World { get; private set; }
        public WorldStageComponent WorldStage { get; private set; }
        public RpcContext RpcContext { get; private set; }

        public UserRepo(RpcContext rpcContext, IRepository repository): base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Player = new PlayerComponent(this, Repository);
            PlayerDetail = new PlayerDetailComponent(this, Repository);
            Point = new PointComponent(this, Repository);
            Ticket = new TicketComponent(this, Repository);
            Cookie = new CookieComponent(this, Repository);
            KingdomStructure = new KingdomStructureComponent(this, Repository);
            KingdomDeco = new KingdomDecoComponent(this, Repository);
            KingdomMap = new KingdomMapComponent(this, Repository);
            Item = new ItemComponent(this, Repository);
            World = new WorldComponent(this, Repository);
            WorldStage = new WorldStageComponent(this, Repository);
        }
    }
}

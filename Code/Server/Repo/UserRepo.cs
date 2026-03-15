using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

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
            Player = new PlayerComponent(this, _repository);
            PlayerDetail = new PlayerDetailComponent(this, _repository);
            Point = new PointComponent(this, _repository);
            Ticket = new TicketComponent(this, _repository);
            Cookie = new CookieComponent(this, _repository);
            KingdomStructure = new KingdomStructureComponent(this, _repository);
            KingdomDeco = new KingdomDecoComponent(this, _repository);
            KingdomMap = new KingdomMapComponent(this, _repository);
            Item = new ItemComponent(this, _repository);
            World = new WorldComponent(this, _repository);
            WorldStage = new WorldStageComponent(this, _repository);
        }
    }
}

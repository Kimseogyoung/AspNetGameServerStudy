using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;
using WebStudyServer.Repo.Cache;

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

        public UserRepo(RpcContext rpcContext, ICacheLayer cacheLayer)
        {
            RpcContext = rpcContext;
            _cacheLayer = cacheLayer;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            IDbLayer dbLayer;
            if (APP.Cfg.DbType == DbType.InMemory)
            {
                dbLayer = new InMemoryDbLayer(_dbFactory, _cacheLayer);
            }
            else
            {
                dbLayer = new DbLayer(_cacheLayer, _dbFactory);
            }
            Player = new PlayerComponent(this, dbLayer);
            PlayerDetail = new PlayerDetailComponent(this, dbLayer);
            Point = new PointComponent(this, dbLayer);
            Ticket = new TicketComponent(this, dbLayer);
            Cookie = new CookieComponent(this, dbLayer);
            KingdomStructure = new KingdomStructureComponent(this, dbLayer);
            KingdomDeco = new KingdomDecoComponent(this, dbLayer);
            KingdomMap = new KingdomMapComponent(this, dbLayer);
            Item = new ItemComponent(this, dbLayer);
            World = new WorldComponent(this, dbLayer);
            WorldStage = new WorldStageComponent(this, dbLayer);
        }

        private readonly ICacheLayer _cacheLayer;
    }
}

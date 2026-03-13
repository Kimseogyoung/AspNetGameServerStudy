using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Repo
{
    public class UserRepo : RepoBase
    {
        public PlayerComponent Player => _playerComponent;
        public PlayerDetailComponent PlayerDetail => _playerDetailComponent;
        public PointComponent Point => _pointComponent;
        public TicketComponent Ticket => _ticketComponent;
        public CookieComponent Cookie => _cookieComponent;
        public ItemComponent Item => _itemComponent;
        public KingdomStructureComponent KingdomStructure => _kingdomStructureComponent;
        public KingdomDecoComponent KingdomDeco => _kingdomDecoComponent;
        public KingdomMapComponent KingdomMap => _kingdomTileMapComponent;
        public WorldComponent World => _worldComponent;
        public WorldStageComponent WorldStage => _worldStageComponent;
        public RpcContext RpcContext { get; private set; }

        public UserRepo(RpcContext rpcContext, ICacheLayer cacheLayer)
        {
            RpcContext = rpcContext;
            _cacheLayer = cacheLayer;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            var dbLayer = new DbLayer(_cacheLayer, _dbFactory);
            _playerComponent = new PlayerComponent(this, dbLayer);
            _playerDetailComponent = new PlayerDetailComponent(this, dbLayer);
            _pointComponent = new PointComponent(this, dbLayer);
            _ticketComponent = new TicketComponent(this, dbLayer);
            _cookieComponent = new CookieComponent(this, dbLayer);
            _kingdomStructureComponent = new KingdomStructureComponent(this, dbLayer);
            _kingdomDecoComponent = new KingdomDecoComponent(this, dbLayer);
            _kingdomTileMapComponent = new KingdomMapComponent(this, dbLayer);
            _itemComponent = new ItemComponent(this, dbLayer);
            _worldComponent = new WorldComponent(this, dbLayer);
            _worldStageComponent = new WorldStageComponent(this, dbLayer);
        }

        private readonly ICacheLayer _cacheLayer;

        private PlayerComponent _playerComponent;
        private PlayerDetailComponent _playerDetailComponent;
        private PointComponent _pointComponent;
        private TicketComponent _ticketComponent;
        private CookieComponent _cookieComponent;
        private ItemComponent _itemComponent;
        private KingdomStructureComponent _kingdomStructureComponent;
        private KingdomDecoComponent _kingdomDecoComponent;
        private KingdomMapComponent _kingdomTileMapComponent;
        private WorldComponent _worldComponent;
        private WorldStageComponent _worldStageComponent;
    }
}

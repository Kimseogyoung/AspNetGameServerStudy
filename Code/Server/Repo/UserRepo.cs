using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

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
        public KingdomTileMapComponent KingdomTileMap => _kingdomTileMapComponent;
        public PlacedKingdomItemComponent PlacedKingdomItem => _placedKingdomItemComponent;
        public RpcContext RpcContext { get; private set; }

        public UserRepo(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            _playerComponent = new PlayerComponent(this, _executor);
            _playerDetailComponent = new PlayerDetailComponent(this, _executor);
            _pointComponent = new PointComponent(this, _executor);
            _ticketComponent = new TicketComponent(this, _executor);
            _cookieComponent = new CookieComponent(this, _executor);
            _kingdomStructureComponent = new KingdomStructureComponent(this, _executor);
            _kingdomDecoComponent = new KingdomDecoComponent(this, _executor);
            _kingdomTileMapComponent = new KingdomTileMapComponent(this, _executor);
            _itemComponent = new ItemComponent(this, _executor);
            _placedKingdomItemComponent = new PlacedKingdomItemComponent(this, _executor);
        }

        public static UserRepo CreateInstance(RpcContext rpcContext)
        {
            var userRepo = new UserRepo(rpcContext);
            return userRepo;
        }

        protected override List<string> _dbConnStrList => APP.Cfg.UserDbConnectionStrList;


        private PlayerComponent _playerComponent;
        private PlayerDetailComponent _playerDetailComponent;
        private PointComponent _pointComponent;
        private TicketComponent _ticketComponent;
        private CookieComponent _cookieComponent;
        private ItemComponent _itemComponent;
        private KingdomStructureComponent _kingdomStructureComponent;
        private KingdomDecoComponent _kingdomDecoComponent;
        private KingdomTileMapComponent _kingdomTileMapComponent;
        private PlacedKingdomItemComponent _placedKingdomItemComponent;
    }
}

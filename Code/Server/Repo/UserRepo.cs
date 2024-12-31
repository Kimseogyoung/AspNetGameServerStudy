using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.GAME;

namespace WebStudyServer.Repo
{
    public class UserRepo : RepoBase
    {
        public PlayerComponent Player => _playerComponent;
        public PlayerDetailComponent PlayerDetail => _playerDetailComponent;
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
        }

        public static UserRepo CreateInstance(RpcContext rpcContext)
        {
            var userRepo = new UserRepo(rpcContext);
            return userRepo;
        }

        protected override List<string> _dbConnStrList => APP.Cfg.UserDbConnectionStrList;


        private PlayerComponent _playerComponent;
        private PlayerDetailComponent _playerDetailComponent;
    }
}

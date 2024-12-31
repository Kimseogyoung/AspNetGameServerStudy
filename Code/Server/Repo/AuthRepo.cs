using Dapper;
using System.Numerics;
using System.Threading.Channels;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;

namespace WebStudyServer.Repo
{
    public class AuthRepo : RepoBase
    {
        public AccountComponent Account => _accountComponent;
        public SessionComponent Session => _sessionComponent;
        public DeviceComponent Device => _deviceComponent;
        public ChannelComponent Channel => _channelComponent;
        public PlayerMapComponent PlayerMap => _playerMapComponent;

        public RpcContext RpcContext {  get; private set; }
        protected override List<string> _dbConnStrList => APP.Cfg.AuthDbConnectionStrList;
        public AuthRepo(RpcContext rpcContext)
        {
            //Transation Filter에서 Init중
            //Init(rpcContext.ShardId);

            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        { 
            // TODO: Lazy
            _accountComponent = new AccountComponent(this, _executor);
            _sessionComponent = new SessionComponent(this, _executor);
            _deviceComponent = new DeviceComponent(this, _executor);
            _channelComponent = new ChannelComponent(this, _executor);
            _playerMapComponent = new PlayerMapComponent(this, _executor);
        }

        public static AuthRepo CreateInstance(RpcContext rpcContext)
        {
            var authRepo = new AuthRepo(rpcContext);
            authRepo.Init(0);
            return authRepo;
        }

        #region PLAYER_MAP
        
        #endregion

        private AccountComponent _accountComponent;
        private SessionComponent _sessionComponent;
        private DeviceComponent _deviceComponent;
        private ChannelComponent _channelComponent;
        private PlayerMapComponent _playerMapComponent;
    }
}

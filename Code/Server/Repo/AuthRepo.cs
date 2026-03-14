using System.Numerics;
using System.Threading.Channels;
using Dapper;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;

namespace WebStudyServer.Repo
{
    public class AuthRepo : RepoBase
    {
        public AccountComponent Account { get; private set; }
        public SessionComponent Session { get; private set; }
        public DeviceComponent Device { get; private set; }
        public ChannelComponent Channel { get; private set; }
        public PlayerMapComponent PlayerMap { get; private set; }

        public RpcContext RpcContext { get; private set; }
        public AuthRepo(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Account = new AccountComponent(this, _dbFactory);
            Session = new SessionComponent(this, _dbFactory);
            Device = new DeviceComponent(this, _dbFactory);
            Channel = new ChannelComponent(this, _dbFactory);
            PlayerMap = new PlayerMapComponent(this, _dbFactory);
        }

        #region PLAYER_MAP

        #endregion

    }
}

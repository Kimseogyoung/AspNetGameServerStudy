using ServerCore;
using ServerCore.Repo.Database;
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

        public IGameContext RpcContext { get; private set; }
        public AuthRepo(IGameContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Account = new AccountComponent(this, Repository);
            Session = new SessionComponent(this, Repository);
            Device = new DeviceComponent(this, Repository);
            Channel = new ChannelComponent(this, Repository);
            PlayerMap = new PlayerMapComponent(this, Repository);
        }

    }
}

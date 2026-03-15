using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Component;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

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
        public AuthRepo(RpcContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)
        {
            RpcContext = rpcContext;
        }

        protected override void PrepareComp()
        {
            // TODO: Lazy
            Account = new AccountComponent(this, _repository);
            Session = new SessionComponent(this, _repository);
            Device = new DeviceComponent(this, _repository);
            Channel = new ChannelComponent(this, _repository);
            PlayerMap = new PlayerMapComponent(this, _repository);
        }

    }
}

using Proto;
using ServerCore.Helper;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class ChannelComponent : AuthComponentBase
    {
        public ChannelComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public bool TryGetActive(ulong accountId, out ChannelManager mgrChannel)
        {
            mgrChannel = null;
            var mdlActiveChannel = GetList(accountId).FirstOrDefault(x => x.State == EChannelState.ACTIVE);
            if (mdlActiveChannel == null) return false;
            mgrChannel = new ChannelManager(_authRepo, mdlActiveChannel);
            return true;
        }

        public ChannelManager Get(string key)
        {
            ReqHelper.ValidContext(TryGet(key, out var mgrChannel), "NOT_FOUND_CHANNEL", () => new { ChannelKey = key });
            return mgrChannel;
        }

        public bool TryGet(string key, out ChannelManager mgrChannel)
        {
            mgrChannel = null;
            var mdlChannel = GetMdl(db => db.SelectByPk<ChannelModel>(new { Key = key }));
            if (mdlChannel == null) return false;
            mgrChannel = new ChannelManager(_authRepo, mdlChannel);
            return true;
        }

        public ChannelManager Create(ulong accountId, EChannelType type, string channelKey = "")
        {
            switch (type)
            {
                case EChannelType.GUEST:
                    channelKey = IdHelper.GenerateGuidKey();
                    break;
            }

            var repoChannel = CreateMdl(new ChannelModel
            {
                Key = channelKey,
                AccountId = accountId,
                Type = type,
                State = EChannelState.ACTIVE,
                Token = ""
            });

            return new ChannelManager(_authRepo, repoChannel);
        }

        public List<ChannelModel> GetList(ulong accountId)
        {
            return GetMdlList(db => db.SelectListByConditions<ChannelModel>(new { AccountId = accountId }).ToList());
        }

        public void Update(ChannelModel mdlChannel)
        {
            UpdateMdl(mdlChannel);
        }
    }
}

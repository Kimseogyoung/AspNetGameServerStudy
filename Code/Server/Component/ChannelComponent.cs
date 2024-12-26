using Proto;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class ChannelComponent : AuthComponentBase
    {
        public ChannelComponent(AuthRepo authRepo) : base(authRepo)
        {
        }

        public bool TryGetActiveChannel(ulong accountId, out ChannelManager mgrChannel)
        {
            mgrChannel = null;

            var mdlChannelList = _authRepo.GetChannelList(accountId);
            var mdlActiveChannel = mdlChannelList.Where(x => x.State == EChannelState.ACTIVE).FirstOrDefault();
            if (mdlActiveChannel == null)
                return false;

            mgrChannel = new ChannelManager(_authRepo, mdlActiveChannel);
            return true;
        }

        public ChannelManager GetChannel(string key)
        {
            ReqHelper.ValidContext(TryGetChannel(key, out var mgrChannel), "NOT_FOUND_CHANNEL", () => new { ChannelKey = key });
            return mgrChannel;
        }

        public bool TryGetChannel(string key, out ChannelManager mgrChannel)
        {
            mgrChannel = null;

            if (!_authRepo.TryGetChannel(key, out var mdlChannel))
            {
                return false;
            }

            mgrChannel = new ChannelManager(_authRepo, mdlChannel);
            return true;
        }

        public ChannelManager CreateChannel(ulong accountId, EChannelType type, string channelKey = "")
        {
            switch (type)
            {
                case EChannelType.GUEST:
                    channelKey = IdHelper.GenerateGuidKey();
                    break;
            }

            var newChannel = new ChannelModel
            {
                Key = channelKey,
                AccountId = accountId,
                Type = type,
                State = EChannelState.ACTIVE,
                Token = ""
            };

            var repoChannel = _authRepo.CreateChannel(newChannel);
            var mgrChannel = new ChannelManager(_authRepo, repoChannel);
            return mgrChannel;
        }
    }
}

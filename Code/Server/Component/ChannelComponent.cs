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

        public bool TryGetChannel(string key, out ChannelManager mgrAccount)
        {
            mgrAccount = null;

            var repoChannel = _authRepo.GetChannel(key);
            if (repoChannel == null)
                return false;

            mgrAccount = new ChannelManager(_authRepo, repoChannel);
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

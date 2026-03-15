using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class ChannelComponent : AuthComponentBase
    {
        public static class Key
        {
            // point lookup 전용 (TryGet/Get 핫패스)
            public static CacheKey Single(string channelKey) => CacheKey.For<ChannelModel>(channelKey);

            // GetList 전용 — ICacheSession prefix 계약 준수:
            //   ListItem이 List의 prefix를 포함하도록 accountId를 앞에 둔다.
            //   List(456)             → "ChannelModel:456"
            //   ListItem(456, "abc")  → "ChannelModel:456:abc"  StartsWith("ChannelModel:456") ✅
            public static CacheKey List(ulong accountId) => CacheKey.For<ChannelModel>(accountId);
            public static CacheKey ListItem(ulong accountId, string channelKey) => CacheKey.For<ChannelModel>(accountId, channelKey);
        }

        public ChannelComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public bool TryGetActive(ulong accountId, out ChannelManager mgrChannel)
        {
            mgrChannel = null;
            var mdlActiveChannel = GetList(accountId).Where(x => x.State == EChannelState.ACTIVE).FirstOrDefault();
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
            var mdlChannel = GetMdl(Key.Single(key), db => db.SelectByPk<ChannelModel>(new { Key = key }));
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
            }, e => Key.Single(e.Key));

            return new ChannelManager(_authRepo, repoChannel);
        }

        public List<ChannelModel> GetList(ulong accountId)
            => GetMdlListByAccountId<ChannelModel>(Key.List(accountId), accountId, e => Key.ListItem(accountId, e.Key));

        public void Update(ChannelModel mdlChannel)
        {
            UpdateMdl(mdlChannel, Key.Single(mdlChannel.Key));
        }
    }
}

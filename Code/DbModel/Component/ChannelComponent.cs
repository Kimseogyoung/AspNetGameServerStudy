using Proto;
using ServerCore.Helper;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using ServerCore.Extension;
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

        public async Task<(bool Found, ChannelManager? Value)> TryGetActiveAsync(ulong accountId)
        {
            var list = await GetListAsync(accountId);
            var mdlActiveChannel = list.FirstOrDefault(x => x.State == EChannelState.ACTIVE);
            return mdlActiveChannel == null ? (false, null) : (true, new ChannelManager(_authRepo, mdlActiveChannel));
        }

        public async Task<ChannelManager> GetAsync(string key)
        {
            var (found, mgrChannel) = await TryGetAsync(key);
            ReqHelper.ValidContext(found, "NOT_FOUND_CHANNEL", () => new { ChannelKey = key });
            return mgrChannel;
        }

        public async Task<(bool Found, ChannelManager? Value)> TryGetAsync(string key)
        {
            var mdlChannel = await GetMdlAsync(db => db.SelectByPkAsync<ChannelModel>(new { Key = key }));
            return mdlChannel == null ? (false, null) : (true, new ChannelManager(_authRepo, mdlChannel));
        }

        public async Task<ChannelManager> CreateAsync(ulong accountId, EChannelType type, string channelKey = "")
        {
            switch (type)
            {
                case EChannelType.GUEST:
                    channelKey = IdHelper.GenerateGuidKey();
                    break;
            }

            var repoChannel = await CreateMdlAsync(new ChannelModel
            {
                Key = channelKey,
                AccountId = accountId,
                Type = type,
                State = EChannelState.ACTIVE,
                Token = ""
            });

            return new ChannelManager(_authRepo, repoChannel);
        }

        public Task<List<ChannelModel>> GetListAsync(ulong accountId)
        {
            return GetMdlListAsync<ChannelModel>(async db => (await db.SelectListByConditionsAsync<ChannelModel>(new { AccountId = accountId })).ToList());
        }

        public Task UpdateAsync(ChannelModel mdlChannel)
        {
            return UpdateMdlAsync(mdlChannel);
        }
    }
}

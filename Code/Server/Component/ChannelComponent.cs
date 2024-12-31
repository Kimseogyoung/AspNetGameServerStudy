using Proto;
using System.Threading.Channels;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class ChannelComponent : AuthComponentBase
    {
        public ChannelComponent(AuthRepo authRepo, DBSqlExecutor executor) : base(authRepo, executor)
        {
        }
    

        public bool TryGetActive(ulong accountId, out ChannelManager mgrChannel)
        {
            mgrChannel = null;

            var mdlChannelList = GetList(accountId);
            var mdlActiveChannel = mdlChannelList.Where(x => x.State == EChannelState.ACTIVE).FirstOrDefault();
            if (mdlActiveChannel == null)
                return false;

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
            ChannelModel mdlChannel = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlChannel = sqlConnection.SelectByPk<ChannelModel>(new { Key = key }, transaction);
            });

            mgrChannel = new ChannelManager(_authRepo, mdlChannel);
            return mdlChannel != null;
        }

        public ChannelManager Create(ulong accountId, EChannelType type, string channelKey = "")
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
            
            ChannelModel repoChannel = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                repoChannel = sqlConnection.Insert(newChannel, transaction);
            });

            var mgrChannel = new ChannelManager(_authRepo, repoChannel);
            return mgrChannel;
        }

        public List<ChannelModel> GetList(ulong accountId)
        {
            var mdlChannelList = new List<ChannelModel>();

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlChannelList = sqlConnection.SelectListByConditions<ChannelModel>(new { AccountId = accountId }, transaction).ToList();
            });

            return mdlChannelList;
        }

        public void Update(ChannelModel mdlChannel)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlChannel, transaction);
            });
        }
    }
}

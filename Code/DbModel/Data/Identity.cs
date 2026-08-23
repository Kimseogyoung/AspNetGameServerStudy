using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // accountId를 모르는 Auth 조회. 기기 키/채널 키로 accountId를 얻거나 계정을 새로 만듦.
    // 얻은 accountId로 GameDb.Auth() 호출.
    //
    // 세션도 accountId를 모르는 조회지만 유일하게 캐시를 쓰므로 GameDb.Sessions로 분리.
    public class Identity
    {
        internal Identity(GameDb db)
        {
            _db = db;
        }

        public async Task<(bool Found, DeviceModel Value)> TryGetDeviceAsync(string idfv)
        {
            var device = await Db.ExecuteAsync(db => db.SelectByPkAsync<DeviceModel>(new { Key = idfv }));
            return (device != null, device);
        }

        public async Task<ChannelModel> GetChannelAsync(string key)
        {
            var channel = await Db.ExecuteAsync(db => db.SelectByPkAsync<ChannelModel>(new { Key = key }));
            ReqHelper.ValidContext(channel != null, "NOT_FOUND_CHANNEL", () => new { ChannelKey = key });
            return channel;
        }

        public Task<AccountModel> CreateAccountAsync()
        {
            var account = new AccountModel
            {
                ShardId = 0, // TODO: ShardId
                State = EAccountState.ACTIVE,
                AdditionalPlayerCnt = 0,
                ClientSecret = "",
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow,
            };

            return Db.ExecuteAsync(db => db.InsertAsync(account));
        }

        // Auth DB는 캐시를 안 쓰므로 IRepository의 캐시 경로를 안 지남. 커넥션은 첫 호출에서 열림.
        private IDbSession Db => _db.AuthRepository().Db;

        private readonly GameDb _db;
    }
}

using Proto;
using ServerCore.Helper;
using ServerCore.Model;
using ServerCore.Repo.Database;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 한 계정의 인증 데이터 경계. accountId를 묶어두므로 호출부가 다른 계정을 조회 못함.
    // accountId를 모르는 조회(기기 키/채널 키/계정 생성)는 Identity로.
    //
    // User와 달리 [Entity].ScopeKey 자동 WHERE 안 씀. 기기 키/채널 키 조회에
    // WHERE AccountId가 붙으면 0행이 되기 때문.
    //
    // 여기가 Auth 데이터 전부다. Session만 캐시를 써서 GameDb.Sessions로 분리돼 있음.
    public class AuthScope
    {
        public ulong AccountId { get; }

        internal AuthScope(GameDb db, ulong accountId)
        {
            _db = db;
            AccountId = accountId;
        }

        public async Task<(bool Found, AccountModel Value)> TryGetAccountAsync()
        {
            var account = await Db.ExecuteAsync(db => db.SelectByPkAsync<AccountModel>(new { Id = AccountId }));
            return (account != null, account);
        }

        public async Task<AccountModel> GetAccountAsync()
        {
            var (found, account) = await TryGetAccountAsync();
            ReqHelper.ValidContext(found, "NOT_FOUND_ACCOUNT", () => new { AccountId });
            return account;
        }

        // ACTIVE 필터는 ChannelQueries.Active()
        public Task<List<ChannelModel>> GetChannelListAsync()
        {
            return Db.ExecuteAsync(async db => (await db.SelectListByConditionsAsync<ChannelModel>(new { AccountId })).ToList());
        }

        public Task<DeviceModel> CreateDeviceAsync(string idfv)
        {
            return CreateAsync(new DeviceModel
            {
                Key = idfv,
                Idfa = "",
                AccountId = AccountId,
                State = EDeviceState.ACTIVE,
                Country = "",
                GeoIpCountry = "",
                Language = "",
            });
        }

        // 계정 -> 플레이어 인덱스. 샤드를 모르는 상태에서 플레이어를 찾을 때 쓴다.
        public Task<PlayerMapModel> CreatePlayerMapAsync(ulong playerId, int shardId)
        {
            return CreateAsync(new PlayerMapModel
            {
                AccountId = AccountId,
                PlayerId = playerId,
                ShardId = shardId,
            });
        }

        public Task<ChannelModel> CreateChannelAsync(EChannelType type, string channelKey = "")
        {
            switch (type)
            {
                case EChannelType.GUEST:
                    channelKey = IdHelper.GenerateGuidKey();
                    break;
            }

            return CreateAsync(new ChannelModel
            {
                Key = channelKey,
                AccountId = AccountId,
                Type = type,
                State = EChannelState.ACTIVE,
                Token = "",
            });
        }

        private Task<T> CreateAsync<T>(T entity) where T : ModelBase
        {
            entity.CreateTime = entity.UpdateTime = DateTime.UtcNow;
            return Db.ExecuteAsync(db => db.InsertAsync(entity));
        }

        // Auth DB는 캐시를 안 쓰므로 세션을 직접 연다. 커넥션은 첫 호출에서 열림.
        private IDbSession Db => _db.SessionFor(DbConnectionResolver.Auth());

        private readonly GameDb _db;
    }
}

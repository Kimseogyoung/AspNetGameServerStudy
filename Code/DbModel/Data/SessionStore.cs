using Proto;
using ServerCore;
using ServerCore.Helper;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 세션은 Auth 에서 유일하게 캐시를 쓴다. 매 요청의 인증 경로라 DB 로 내려가면 전 요청이 느려진다.
    // 그래서 Identity(무캐시)와 합치지 않고 따로 둔다.
    //
    // 키가 둘이다. 세션 키 -> accountId 는 포인터, accountId -> SessionModel 은 값 + sliding TTL.
    // 세션 키 조회는 accountId 를 모르는 상태의 조회라 AuthScope 밖이다.
    public class SessionStore
    {
        internal SessionStore(GameDb db)
        {
            _db = db;
        }

        public async Task<(bool Found, SessionModel Value)> TryGetByKeyAsync(string sessionKey)
        {
            var pointerKey = PointerKey(sessionKey);
            var cached = await Cache.TryGetAsync<ulong>(pointerKey, Ttl);
            if (cached.Hit)
            {
                return await TryGetByAccountIdAsync(cached.Value);
            }

            var mdlSession = await Db.ExecuteAsync(db => db.SelectByConditionsAsync<SessionModel>(new { Key = sessionKey }));
            if (mdlSession == null)
            {
                return (false, null);
            }

            await SetBothKeysAsync(mdlSession);
            return (true, mdlSession);
        }

        public async Task<(bool Found, SessionModel Value)> TryGetByAccountIdAsync(ulong accountId)
        {
            var valueKey = ValueKey(accountId);
            var cached = await Cache.TryGetAsync<SessionModel>(valueKey, Ttl);
            if (cached.Hit)
            {
                // 히트해도 포인터를 다시 찍는다. 값 캐시만 살아남고 포인터가 만료된 경우를 메운다.
                await Cache.SetAsync(PointerKey(cached.Value.Key), accountId, Ttl);
                return (true, cached.Value);
            }

            var mdlSession = await Db.ExecuteAsync(db => db.SelectByConditionsAsync<SessionModel>(new { AccountId = accountId }));
            if (mdlSession == null)
            {
                return (false, null);
            }

            await SetBothKeysAsync(mdlSession);
            return (true, mdlSession);
        }

        public async Task<SessionModel> CreateAsync(ulong accountId, int shardId, SessionStamp stamp)
        {
            var mdlSession = new SessionModel
            {
                Key = IdHelper.GenerateGuidKey(),
                AccountId = accountId,
                ShardId = shardId,
                PublicIp = stamp.Ip,
                DeviceKey = "",
                State = ESessionState.NONE,
                ClientSecret = "",
                EncryptSecret = "",
                EncryptIV = "",
                ExpireTimestamp = 0,
                PlayerId = 0,
            };

            mdlSession.CreateTime = mdlSession.UpdateTime = DateTime.UtcNow;
            mdlSession = await Db.ExecuteAsync(db => db.InsertAsync(mdlSession));
            await SetBothKeysAsync(mdlSession);
            return mdlSession;
        }

        // 세션을 시작하고 새 키를 돌려준다. 컨텍스트 갱신은 호출부의 일이다.
        //
        // 키 로테이션의 이전 키를 여기서 잡는 이유는, 호출부가 모델을 바꾼 뒤에 잡으면
        // 옛 포인터가 무효화되지 않아 로테이션된 뒤에도 옛 키로 인증이 통과하기 때문이다.
        public async Task<string> StartAsync(SessionModel mdlSession, SessionStamp stamp)
        {
            var befSessionKey = mdlSession.Key;
            var aftSessionKey = mdlSession.Start(stamp, Config<GameConfig>.Get().SessionExpireSpan);
            await SaveAsync(befSessionKey, mdlSession);
            return aftSessionKey;
        }

        // 키가 그대로인 저장. 키 로테이션은 StartAsync 만 할 수 있게 두 인자 버전을 닫아뒀다 -
        // 호출부가 이전 키를 못 넘기면 옛 포인터가 남아 로테이션 뒤에도 옛 키로 인증이 통과한다.
        public Task SaveAsync(SessionModel mdlSession)
        {
            return SaveAsync(mdlSession.Key, mdlSession);
        }

        private async Task SaveAsync(string befSessionKey, SessionModel mdlSession)
        {
            if (befSessionKey != mdlSession.Key)
            {
                await Cache.InvalidateAsync(PointerKey(befSessionKey));
            }

            mdlSession.UpdateTime = DateTime.UtcNow;
            await Db.ExecuteAsync(db => db.UpdateAsync(mdlSession));
            await SetBothKeysAsync(mdlSession);
        }

        public async Task LogoutAsync(SessionModel mdlSession)
        {
            await Cache.InvalidateAsync(ValueKey(mdlSession.AccountId));
            await Cache.InvalidateAsync(PointerKey(mdlSession.Key));
        }

        private async Task SetBothKeysAsync(SessionModel mdlSession)
        {
            await Cache.SetAsync(ValueKey(mdlSession.AccountId), mdlSession, Ttl);
            await Cache.SetAsync(PointerKey(mdlSession.Key), mdlSession.AccountId, Ttl);
        }

        private static CacheKey PointerKey(string sessionKey)
            => CacheKey.For(CacheKeyTags.SessionModel, "AccountBySessionKey", sessionKey);

        private static CacheKey ValueKey(ulong accountId)
            => CacheKey.For(CacheKeyTags.SessionModel, "AccountId", accountId);

        private static TimeSpan Ttl => Config<CoreConfig>.Get().CacheDefaultTtl;

        private ICacheSession Cache => _db.Cache;
        private IDbSession Db => _db.SessionFor(DbConnectionResolver.Auth());

        private readonly GameDb _db;
    }
}

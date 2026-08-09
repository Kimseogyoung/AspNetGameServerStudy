using Proto;
using ServerCore.Repo.Database;
using ServerCore;
using WebStudyServer.Base;
using ServerCore.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class SessionComponent : AuthComponentBase
    {
        public static class Key
        {
            internal static TimeSpan Ttl => Config<CoreConfig>.Get().CacheDefaultTtl;

            public static CacheKey AccountIdBySessionKey(string key)
                => CacheKey.For(CacheKeyTags.SessionModel, "AccountBySessionKey", key);

            public static CacheKey SessionByAccountId(ulong accountId)
                => CacheKey.For(CacheKeyTags.SessionModel, "AccountId", accountId);
        }

        public SessionComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public async Task<SessionManager?> TryGetByKeyAsync(string key)
        {
            var accountIdBySessionKey = Key.AccountIdBySessionKey(key);

            var cached = await _repository.Cache.TryGetAsync<ulong>(accountIdBySessionKey, Key.Ttl);
            if (!cached.Hit)
            {
                var dbSession = await GetMdlAsync<SessionModel>(db => db.SelectByConditionsAsync<SessionModel>(new { Key = key }));
                if (dbSession == null)
                {
                    return null;
                }

                await _repository.Cache.SetAsync(Key.SessionByAccountId(dbSession.AccountId), dbSession, Key.Ttl);
                await _repository.Cache.SetAsync(accountIdBySessionKey, dbSession.AccountId, Key.Ttl);
                return new SessionManager(_authRepo, dbSession);
            }

            return await TryGetByAccountIdAsync(cached.Value);
        }

        public async Task<SessionManager?> TryGetByAccountIdAsync(ulong accountId)
        {
            var mdlSession = await GetByAccountIdAsync(accountId);
            return mdlSession == null ? null : new SessionManager(_authRepo, mdlSession);
        }

        private async Task<SessionModel?> GetByAccountIdAsync(ulong accountId)
        {
            var sessionByAccountIdKey = Key.SessionByAccountId(accountId);
            var session = await GetMdlWithCacheAsync<SessionModel>(
                    sessionByAccountIdKey,
                    db => db.SelectByConditionsAsync<SessionModel>(new { AccountId = accountId }),
                    Key.Ttl);

            if (session != null)
            {
                // 포인터 업데이트
                var accountIdBySessionKey = Key.AccountIdBySessionKey(session.Key);
                await _repository.Cache.SetAsync(accountIdBySessionKey, accountId, Key.Ttl);
            }

            return session;
        }

        public async Task<SessionManager> TouchAsync(ulong accountId)
        {
            var mdlSession = await GetByAccountIdAsync(accountId);
            if (mdlSession == null)
            {
                mdlSession = await CreateMdlAsync(new SessionModel
                {
                    Key = IdHelper.GenerateGuidKey(),
                    AccountId = accountId,
                    PublicIp = _authRepo.RpcContext.Ip,
                    ShardId = _authRepo.RpcContext.ShardId,
                    State = ESessionState.NONE,
                    ClientSecret = "",
                    EncryptSecret = "",
                    EncryptIV = "",
                    ExpireTimestamp = 0,
                    PlayerId = 0,
                    DeviceKey = "",
                });

                var accountIdKey = Key.SessionByAccountId(accountId);
                await _repository.Cache.SetAsync(accountIdKey, mdlSession, Key.Ttl);
                await _repository.Cache.SetAsync(Key.AccountIdBySessionKey(mdlSession.Key), accountId, Key.Ttl);
            }
            return new SessionManager(_authRepo, mdlSession);
        }

        // primary 캐시 갱신. 키 로테이션 시 이전 포인터 제거 + 새 포인터 등록.
        public async Task UpdateAsync(string befSessionKey, SessionModel mdlSession)
        {
            if (befSessionKey != mdlSession.Key)
            {
                await _repository.Cache.InvalidateAsync(Key.AccountIdBySessionKey(befSessionKey));
                await _repository.Cache.SetAsync(Key.AccountIdBySessionKey(mdlSession.Key), mdlSession.AccountId, Key.Ttl);
            }

            await UpdateMdlAsync(mdlSession);
            await _repository.Cache.SetAsync(Key.SessionByAccountId(mdlSession.AccountId), mdlSession, Key.Ttl);
        }

        // 두 키 즉시 제거
        public async Task LogoutAsync(SessionModel mdlSession)
        {
            await _repository.Cache.InvalidateAsync(Key.SessionByAccountId(mdlSession.AccountId));
            await _repository.Cache.InvalidateAsync(Key.AccountIdBySessionKey(mdlSession.Key));
        }
    }
}

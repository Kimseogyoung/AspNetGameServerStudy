using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class SessionComponent : AuthComponentBase
    {
        public static class Key
        {
            internal static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

            public static CacheKey AccountIdBySessionKey(string key)
                => CacheKey.For<SessionModel>("AccountBySessionKey", key); // 제네릭 T떼기

            public static CacheKey SessionByAccountId(ulong accountId)
                => CacheKey.For<SessionModel>("AccountId", accountId);
        }

        public SessionComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public bool TryGetByKey(string key, out SessionManager mgrSession)
        {
            mgrSession = null;

            var accountIdBySessionKey = Key.AccountIdBySessionKey(key);

            if (!_repository.Cache.TryGet<ulong>(accountIdBySessionKey, out var cachedAccountId))
            {
                var dbSession = GetMdl<SessionModel>(db => db.SelectByConditions<SessionModel>(new { Key = key }));
                if (dbSession == null)
                {
                    return false;
                }

                _repository.Cache.Set(Key.SessionByAccountId(dbSession.AccountId), dbSession, Key.Ttl);
                _repository.Cache.Set(accountIdBySessionKey, dbSession.AccountId, Key.Ttl);
                mgrSession = new SessionManager(_authRepo, dbSession);
                return true;
            }

            return TryGetByAccountId(cachedAccountId, out mgrSession);
        }

        public bool TryGetByAccountId(ulong accountId, out SessionManager mgrSession)
        {
            mgrSession = null;
            var mdlSession = GetByAccountId(accountId);
            if (mdlSession == null)
            {
                return false;
            }
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        private SessionModel? GetByAccountId(ulong accountId)
        {
            var sessionByAccountIdKey = Key.SessionByAccountId(accountId);
            var session = GetMdlWithCache<SessionModel>(
                    sessionByAccountIdKey,
                    db => db.SelectByConditions<SessionModel>(new { AccountId = accountId }));

            if (session != null)
            {
                // 포인터 업데이트
                var accountIdBySessionKey = Key.AccountIdBySessionKey(session.Key);
                _repository.Cache.Set(accountIdBySessionKey, accountId, Key.Ttl);
            }

            return session;
        }

        public SessionManager Touch(ulong accountId)
        {
            var mdlSession = GetByAccountId(accountId);
            if (mdlSession == null)
            {
                mdlSession = CreateMdl(new SessionModel
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
                _repository.Cache.Set(accountIdKey, mdlSession, Key.Ttl);
                _repository.Cache.Set(Key.AccountIdBySessionKey(mdlSession.Key), accountId, Key.Ttl);
            }
            return new SessionManager(_authRepo, mdlSession);
        }

        // primary 캐시 갱신. 키 로테이션 시 이전 포인터 제거 + 새 포인터 등록.
        public void Update(string befSessionKey, SessionModel mdlSession)
        {
            if (befSessionKey != mdlSession.Key)
            {
                _repository.Cache.Invalidate(Key.AccountIdBySessionKey(befSessionKey));
                _repository.Cache.Set(Key.AccountIdBySessionKey(mdlSession.Key), mdlSession.AccountId, Key.Ttl);
            }

            UpdateMdl(mdlSession);
            _repository.Cache.Set(Key.SessionByAccountId(mdlSession.AccountId), mdlSession, Key.Ttl);
        }

        // 두 키 즉시 제거
        public void Logout(SessionModel mdlSession)
        {
            _repository.Cache.Invalidate(Key.SessionByAccountId(mdlSession.AccountId));
            _repository.Cache.Invalidate(Key.AccountIdBySessionKey(mdlSession.Key));
        }
    }
}

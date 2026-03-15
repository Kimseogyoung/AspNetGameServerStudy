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
    public class SessionComponent : AuthComponentBase
    {
        // 단일 캐시 키: Key(GUID) 기준
        // TryGetByKey가 hot path(매 요청 토큰 검증)이므로 ByKey를 캐노니컬로 사용
        // TryGetByAccountId / Touch는 auth flow에서만 호출되므로 캐시 없이 DB 직접 조회
        public static class Key
        {
            public static CacheKey ByKey(string key) => CacheKey.For<SessionModel>(key);
        }

        public SessionComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        // auth flow 전용 — 캐시 없이 DB 직접 조회
        public bool TryGetByAccountId(ulong accountId, out SessionManager mgrSession)
        {
            mgrSession = null;
            var mdlSession = DbSession.Execute(db => db.SelectByConditions<SessionModel>(new { AccountId = accountId }));
            if (mdlSession == null) return false;
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        // hot path (매 요청 토큰 검증) — 캐시 사용
        public bool TryGetByKey(string key, out SessionManager mgrSession)
        {
            mgrSession = null;
            var mdlSession = GetMdl(Key.ByKey(key), db => db.SelectByConditions<SessionModel>(new { Key = key }));
            if (mdlSession == null) return false;
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        // auth flow 전용 — AccountId 조회는 DB 직접, 생성 시 ByKey로 캐시
        public SessionManager Touch(ulong accountId)
        {
            var mdlSession = DbSession.Execute(db => db.SelectByConditions<SessionModel>(new { AccountId = accountId }));

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
                }, e => Key.ByKey(e.Key));
            }

            return new SessionManager(_authRepo, mdlSession);
        }

        public void Update(SessionModel mdlSession)
        {
            UpdateMdl(mdlSession, Key.ByKey(mdlSession.Key));
        }
    }
}

using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class SessionComponent : AuthComponentBase
    {
        public SessionComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        // auth flow 전용 — DB 직접 조회
        public bool TryGetByAccountId(ulong accountId, out SessionManager mgrSession)
        {
            mgrSession = null;
            var mdlSession = DbSession.Execute(db => db.SelectByConditions<SessionModel>(new { AccountId = accountId }));
            if (mdlSession == null) return false;
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        public bool TryGetByKey(string key, out SessionManager mgrSession)
        {
            mgrSession = null;
            var mdlSession = GetMdl(db => db.SelectByConditions<SessionModel>(new { Key = key }));
            if (mdlSession == null) return false;
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        // auth flow 전용 — AccountId 조회는 DB 직접
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
                });
            }

            return new SessionManager(_authRepo, mdlSession);
        }

        public void Update(SessionModel mdlSession)
        {
            UpdateMdl(mdlSession);
        }
    }
}

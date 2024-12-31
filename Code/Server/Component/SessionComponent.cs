using Proto;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class SessionComponent : AuthComponentBase
    {
        public SessionComponent(AuthRepo authRepo, DBSqlExecutor executor) : base(authRepo, executor)
        {
        }

        public bool TryGetByAccountId(ulong accountId, out SessionManager mgrSession)
        {
            mgrSession = null;

            if (!TryGetByAccountIdInternal(accountId, out var mdlSession))
            {
                return false;
            }

            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        public bool TryGetByKey(string key, out SessionManager mgrSession)
        {
            mgrSession = null;

            if(!TryGetByKeyInternal(key, out var mdlSession))
            {
                return false;
            }
           
            mgrSession = new SessionManager(_authRepo, mdlSession);
            return true;
        }

        public SessionManager Touch(ulong accountId)
        {
            if (!TryGetByAccountIdInternal(accountId, out var mdlSession))
            {
                var newSession = new SessionModel
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
                };

                mdlSession = CreateSessionInternal(newSession);

            }
            var mgrSession = new SessionManager(_authRepo, mdlSession);
            return mgrSession;
        }

        public void Update(SessionModel mdlSession)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlSession, transaction);
            });
        }

        private bool TryGetByKeyInternal(string key, out SessionModel outSession)
        {
            SessionModel mdlSession = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlSession = sqlConnection.SelectByConditions<SessionModel>(new { Key = key }, transaction);
            });
            outSession = mdlSession;

            return outSession != null;
        }

        private bool TryGetByAccountIdInternal(ulong accountId, out SessionModel outSession)
        {
            SessionModel mdlSession = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlSession = sqlConnection.SelectByConditions<SessionModel>(new { AccountId = accountId }, transaction);
            });

            outSession = mdlSession;
            return outSession != null;
        }

        private SessionModel CreateSessionInternal(SessionModel inSession)
        {
            SessionModel newSession = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newSession = sqlConnection.Insert(inSession, transaction);
            });

            return newSession; // 새로 생성된 계정 모델 반환
        }
    }
}

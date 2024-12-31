using WebStudyServer.Model;
using WebStudyServer.Repo;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
namespace WebStudyServer.Manager
{
    public class SessionManager : AuthManagerBase<SessionModel>
    {
        public SessionManager(AuthRepo authRepo, SessionModel model) : base(authRepo, model)
        {
        }
        
        public void Start()
        {
            // 세션 시작
            var expireTime = _authRepo.RpcContext.ServerTime + APP.Cfg.SessionExpireSpan;
            var sessionKey = IdHelper.GenerateGuidKey();
            Model.Key = sessionKey;
            Model.State = ESessionState.ACTIVE;
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(expireTime);
            Model.PublicIp = _authRepo.RpcContext.Ip;
            Model.ClientSecret = "";
            Model.DeviceKey = _authRepo.RpcContext.DeviceKey;
            Model.EncryptIV = "";
            Model.EncryptSecret = "";
            _authRepo.Session.Update(Model);

            _authRepo.RpcContext.SetSessionKey(sessionKey);
        }

        public void SetPlayerId(ulong playerId)
        {
            if (Model.PlayerId == playerId)
            {
                return;
            }

            Model.PlayerId = playerId;
            _authRepo.Session.Update(Model);
        }

        public bool IsExpire()
        {
            return Model.State != ESessionState.ACTIVE;
        }

        public bool Extend()
        {
            if (Model.State == ESessionState.EXPIRED)
            {
                return false;
            }

            var expireTime = TimeHelper.TimeStampToDateTime(Model.ExpireTimestamp);
            var serverTime = _authRepo.RpcContext.ServerTime;

            var isExpire = serverTime > expireTime;
            if (!isExpire)
            {
                return false;
            }

            // TODO: 연장
            //

            return true;
        }
/*
        private void RefreshState()
        {
            var expireTime = TimeHelper.TimeStampToDateTime(Model.ExpireTimestamp);
            var serverTime = _authRepo.RpcContext.ServerTime;

            var isExpire = serverTime > expireTime;
            var aftState = isExpire ? ESessionState.EXPIRED : ESessionState.ACTIVE;

            if (Model.State == aftState)
            {
                return;
            }

            Model.State = aftState;
            _authRepo.UpdateSession(Model);
        }*/

    }
}

using Proto;
using ServerCore.Helper;
using WebStudyServer.GAME;
using WebStudyServer.Model;
using WebStudyServer.Repo;
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
            var befSessionKey = Model.Key;
            var aftSessionKey = IdHelper.GenerateGuidKey();
            Model.Key = aftSessionKey;
            Model.State = ESessionState.ACTIVE;
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(expireTime);
            Model.PublicIp = _authRepo.RpcContext.Ip;
            Model.ClientSecret = "";
            Model.DeviceKey = _authRepo.RpcContext.DeviceKey;
            Model.EncryptIV = "";
            Model.EncryptSecret = "";
            _authRepo.Session.Update(befSessionKey, Model);

            _authRepo.RpcContext.SetSessionKey(aftSessionKey);
        }

        public void SetPlayerId(ulong playerId)
        {
            if (Model.PlayerId == playerId)
            {
                return;
            }

            Model.PlayerId = playerId;
            _authRepo.Session.Update(Model.Key, Model);
        }

        public bool IsExpire()
        {
            return Model.State != ESessionState.ACTIVE;
        }

        public bool Extend()
        {
            var serverTime = _authRepo.RpcContext.ServerTime;
            var expireTime = TimeHelper.TimeStampToDateTime(Model.ExpireTimestamp);

            // 만료 시간 경과 → Revival 먼저 시도 (ACTIVE/EXPIRED 모두 동일 처리)
            // Revival을 먼저 시도해야 첫 만료 요청에서 1 round-trip으로 처리됨
            if (Model.State == ESessionState.EXPIRED || serverTime >= expireTime)
            {
                if (TryReviveByDeviceKey(serverTime, expireTime))
                {
                    return true;
                }

                if (Model.State != ESessionState.EXPIRED)
                {
                    Model.State = ESessionState.EXPIRED;
                    _authRepo.Session.Update(Model.Key, Model);
                }

                return false;
            }

            // Half-life: 남은 시간이 절반 초과 → 갱신 불필요 (DB 부하 최적화)
            var remaining = expireTime - serverTime;
            if (remaining > APP.Cfg.SessionExpireSpan / 2)
            {
                return false;
            }

            // 연장: ExpireTimestamp 갱신 → DB + 캐시 TTL 동시 갱신
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(serverTime + APP.Cfg.SessionExpireSpan);
            _authRepo.Session.Update(Model.Key, Model);
            return true;
        }

        public void Expire()
        {
            if (Model.State == ESessionState.EXPIRED)
            {
                return;
            }

            Model.State = ESessionState.EXPIRED;
            _authRepo.Session.Update(Model.Key, Model);
        }

        private bool TryReviveByDeviceKey(DateTime serverTime, DateTime expireTime)
        {
            var reqDeviceKey = _authRepo.RpcContext.DeviceKey;
            if (string.IsNullOrEmpty(reqDeviceKey) || reqDeviceKey != Model.DeviceKey)
            {
                return false;
            }

            // Grace period 초과 여부 확인
            if (serverTime > expireTime + APP.Cfg.SessionGracePeriodSpan)
            {
                return false;
            }

            // 세션 부활: State + ExpireTimestamp 갱신
            Model.State = ESessionState.ACTIVE;
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(serverTime + APP.Cfg.SessionExpireSpan);
            _authRepo.Session.Update(Model.Key, Model);
            return true;
        }

    }
}

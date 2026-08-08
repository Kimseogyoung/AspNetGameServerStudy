using Proto;
using ServerCore;
using ServerCore.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;
namespace WebStudyServer.Manager
{
    public class SessionManager : AuthManagerBase<SessionModel>
    {
        public SessionManager(AuthRepo authRepo, SessionModel model) : base(authRepo, model)
        {
        }

        public async Task StartAsync()
        {
            // 세션 시작
            var expireTime = _authRepo.RpcContext.ServerTime + Config<GameConfig>.Get().SessionExpireSpan;
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
            await _authRepo.Session.UpdateAsync(befSessionKey, Model);

            _authRepo.RpcContext.SetSessionKey(aftSessionKey);
        }

        public async Task SetPlayerIdAsync(ulong playerId)
        {
            if (Model.PlayerId == playerId)
            {
                return;
            }

            Model.PlayerId = playerId;
            await _authRepo.Session.UpdateAsync(Model.Key, Model);
        }

        public bool IsExpire()
        {
            return Model.State != ESessionState.ACTIVE;
        }

        public async Task<bool> ExtendAsync()
        {
            var serverTime = _authRepo.RpcContext.ServerTime;
            var expireTime = TimeHelper.TimeStampToDateTime(Model.ExpireTimestamp);

            // 만료 시간 경과 → Revival 먼저 시도 (ACTIVE/EXPIRED 모두 동일 처리)
            // Revival을 먼저 시도해야 첫 만료 요청에서 1 round-trip으로 처리됨
            if (Model.State == ESessionState.EXPIRED || serverTime >= expireTime)
            {
                if (await TryReviveByDeviceKeyAsync(serverTime, expireTime))
                {
                    return true;
                }

                if (Model.State != ESessionState.EXPIRED)
                {
                    Model.State = ESessionState.EXPIRED;
                    await _authRepo.Session.UpdateAsync(Model.Key, Model);
                }

                return false;
            }

            // Half-life: 남은 시간이 절반 초과 → 갱신 불필요 (DB 부하 최적화)
            var remaining = expireTime - serverTime;
            if (remaining > Config<GameConfig>.Get().SessionExpireSpan / 2)
            {
                return false;
            }

            // 연장: ExpireTimestamp 갱신 → DB + 캐시 TTL 동시 갱신
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(serverTime + Config<GameConfig>.Get().SessionExpireSpan);
            await _authRepo.Session.UpdateAsync(Model.Key, Model);
            return true;
        }

        public async Task ExpireAsync()
        {
            if (Model.State == ESessionState.EXPIRED)
            {
                return;
            }

            Model.State = ESessionState.EXPIRED;
            await _authRepo.Session.UpdateAsync(Model.Key, Model);
        }

        private async Task<bool> TryReviveByDeviceKeyAsync(DateTime serverTime, DateTime expireTime)
        {
            var reqDeviceKey = _authRepo.RpcContext.DeviceKey;
            if (string.IsNullOrEmpty(reqDeviceKey) || reqDeviceKey != Model.DeviceKey)
            {
                return false;
            }

            // Grace period 초과 여부 확인
            if (serverTime > expireTime + Config<GameConfig>.Get().SessionGracePeriodSpan)
            {
                return false;
            }

            // 세션 부활: State + ExpireTimestamp 갱신
            Model.State = ESessionState.ACTIVE;
            Model.ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(serverTime + Config<GameConfig>.Get().SessionExpireSpan);
            await _authRepo.Session.UpdateAsync(Model.Key, Model);
            return true;
        }

    }
}

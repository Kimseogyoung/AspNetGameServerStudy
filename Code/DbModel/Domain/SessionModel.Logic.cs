using Proto;
using ServerCore.Helper;
using WebStudyServer.Data;

namespace WebStudyServer.Model
{
    // 세션의 시간 판단은 전부 여기 모은다. 서버 시각을 인자로 받으므로 순수하다 -
    // 데이터 계층이 RpcContext 를 읽던 것을 호출부로 올린 결과다.
    public partial class SessionModel
    {
        public bool IsExpire()
        {
            return State != ESessionState.ACTIVE;
        }

        // 세션 시작. 키를 새로 발급하고 바뀐 키를 돌려준다.
        public string Start(SessionStamp stamp, TimeSpan expireSpan)
        {
            Key = IdHelper.GenerateGuidKey();
            State = ESessionState.ACTIVE;
            ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(stamp.ServerTime + expireSpan);
            PublicIp = stamp.Ip;
            DeviceKey = stamp.DeviceKey;
            ClientSecret = "";
            EncryptIV = "";
            EncryptSecret = "";
            return Key;
        }

        public bool Expire()
        {
            if (State == ESessionState.EXPIRED)
            {
                return false;
            }

            State = ESessionState.EXPIRED;
            return true;
        }

        // 남은 시간이 절반 이하일 때만 갱신한다. 매 요청 쓰기를 피하려는 것이다.
        public bool TryExtend(SessionStamp stamp, TimeSpan expireSpan)
        {
            var expireTime = TimeHelper.TimeStampToDateTime(ExpireTimestamp);
            if (expireTime - stamp.ServerTime > expireSpan / 2)
            {
                return false;
            }

            ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(stamp.ServerTime + expireSpan);
            return true;
        }

        public bool IsPastExpireTime(DateTime serverTime)
        {
            return State == ESessionState.EXPIRED || serverTime >= TimeHelper.TimeStampToDateTime(ExpireTimestamp);
        }

        // 같은 기기가 유예 기간 안에 돌아오면 되살린다.
        public bool TryRevive(SessionStamp stamp, TimeSpan expireSpan, TimeSpan graceSpan)
        {
            if (string.IsNullOrEmpty(stamp.DeviceKey) || stamp.DeviceKey != DeviceKey)
            {
                return false;
            }

            var expireTime = TimeHelper.TimeStampToDateTime(ExpireTimestamp);
            if (stamp.ServerTime > expireTime + graceSpan)
            {
                return false;
            }

            State = ESessionState.ACTIVE;
            ExpireTimestamp = TimeHelper.DateTimeToTimeStamp(stamp.ServerTime + expireSpan);
            return true;
        }

        public bool SetPlayerId(ulong playerId)
        {
            if (PlayerId == playerId)
            {
                return false;
            }

            PlayerId = playerId;
            return true;
        }
    }
}

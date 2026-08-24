using System.Text.Json;
using Proto;
using Protocol;
using ServerCore;
using ServerCore.Extension;
using ServerCore.Helper;
using ServerCore.Model;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 재화 변동의 기록. DB 원장은 유료 재화에만 두고 나머지는 로그까지만 간다.
    //
    // 쓰기가 실패해도 요청은 실패시키지 않는다. 원장 한 행이 비는 것보다 재화 변동이
    // 통째로 롤백되는 쪽이 나쁘다. 같은 트랜잭션 안에서 삼키므로, 성공할 때는 재화
    // 변동과 원장이 한 덩어리로 커밋된다.
    public static class AuditService
    {
        // 캐시를 바꾼 쪽이 부른다. 안 바뀐 축은 현재 값이 그대로 Bef/Aft 로 들어간다 -
        // 한 행이 그 시점의 두 잔액을 다 담아야 하기 때문이다.
        // internal 인 이유는 변동 지점 밖에서 부를 자리가 없기 때문이다.
        internal static Task WriteCashChangeAsync(
            UserScope userScope, string reason, PlayerDetailModel detail, double befRealCash, double befFreeCash)
        {
            var (actionName, actionDetail) = SplitReason(reason);

            return InsertLogAsync(userScope, new CashChangeLogModel
            {
                SfId = IdHelper.GenerateSfId(),
                ActionName = actionName,
                // ActionNameHash 는 API 해시를 넣을 자리다. 그 기능이 아직 없어 비워 둔다.
                ActionDetail = actionDetail,
                ChgRealCash = (long)(detail.RealCash - befRealCash),
                BefRealCash = (long)befRealCash,
                AftRealCash = (long)detail.RealCash,
                AccRealCash = (long)detail.AccRealCash,
                ChgFreeCash = (long)(detail.FreeCash - befFreeCash),
                BefFreeCash = (long)befFreeCash,
                AftFreeCash = (long)detail.FreeCash,
                AccFreeCash = (long)detail.AccFreeCash,
            }, reason);
        }

        // 가챠 API 호출 하나가 한 행이다. ChgObj 는 결과가 아니라 소모한 재화다.
        // 뽑은 결과는 ExtraData 에 JSON 으로 들어간다. 컬럼이 TEXT 라 연차 수는 제약이 안 된다.
        public static Task WriteGachaAsync(
            UserScope userScope, int scheduleNum, int cnt, ObjValue cost,
            IReadOnlyList<ChangeSet> costChangeList, IReadOnlyList<GachaResultPacket> resultList)
        {
            var real = Find(costChangeList, EObjType.REAL_CASH);
            var free = Find(costChangeList, EObjType.FREE_CASH);

            return InsertLogAsync(userScope, new GachaLogModel
            {
                SfId = IdHelper.GenerateSfId(),
                ScheduleNum = scheduleNum,
                Cnt = cnt,
                ChgRealCash = (int)(real?.Delta ?? 0),
                ChgFreeCash = (int)(free?.Delta ?? 0),
                ChgObjType = cost.Key.Type,
                ChgObjAmount = (int)cost.Value,
                ExtraData = JsonSerializer.Serialize(resultList),
            }, $"GACHA:{scheduleNum}");
        }

        // 실패를 삼키는 것은 이 클래스 전체의 정책이라 이름에 다시 담지 않는다. 클래스 주석 참조.
        private static async Task InsertLogAsync<T>(UserScope userScope, T row, string reason)
            where T : ModelBase, IScopedModel
        {
            try
            {
                await userScope.InsertAsync(row);
            }
            catch (Exception e)
            {
                Logger.Get().Error(e, "AUDIT_WRITE_FAILED Model({Model}) PlayerId({PlayerId}) Reason({Reason})",
                    typeof(T).Name, userScope.PlayerId, reason);
            }
        }

        private static ChangeSet? Find(IReadOnlyList<ChangeSet> changeList, EObjType type)
        {
            foreach (var change in changeList)
            {
                if (change.Type == type)
                {
                    return change;
                }
            }

            return null;
        }

        // reason 은 "NAME:detail" 규약이다. 컬럼이 VARCHAR(30) 이라 잘라 넣는다.
        private static (string Name, string Detail) SplitReason(string reason)
        {
            var text = reason ?? string.Empty;
            var idx = text.IndexOf(':');
            return idx < 0
                ? (Truncate(text), string.Empty)
                : (Truncate(text[..idx]), Truncate(text[(idx + 1)..]));
        }

        private static string Truncate(string text)
        {
            return text.Length <= ColumnLength ? text : text[..ColumnLength];
        }

        private const int ColumnLength = 30;
    }
}

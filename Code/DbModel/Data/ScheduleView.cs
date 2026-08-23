using Proto;
using Protocol;
using ServerCore.Helper;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 기획 데이터(Proto) 위에 운영 변경(Model)을 덮은 결과. DB 를 타지 않으므로 값으로 둔다.
    //
    // Mdl 이 null 이면 운영 중 바꾼 적이 없다는 뜻이고 Prt 의 일정이 그대로 유효하다.
    // 겹치기를 생성자에서 복사하지 않고 그때그때 계산한다 - 복사해두면 Mdl 을 고쳐도 안 따라온다.
    public readonly record struct ScheduleView(ScheduleProto Prt, ScheduleModel Mdl)
    {
        public int Num => Prt.Num;
        public int State => Mdl?.State ?? 0;

        public DateTime ActiveStartTime => Mdl?.ActiveStartTime ?? Prt.ActiveStartTime;
        public DateTime ActiveEndTime => Mdl?.ActiveEndTime ?? Prt.ActiveEndTime;
        public DateTime ContentStartTime => Mdl?.ContentStartTime ?? Prt.ContentStartTime;
        public DateTime ContentEndTime => Mdl?.ContentEndTime ?? Prt.ContentEndTime;

        // 가챠 스케줄일 때만 쓸 수 있다. 다른 타입에 가챠 API 를 부르면 NRE 대신 여기서 걸린다.
        // ScheduleNum 은 클라가 보내므로 출석 스케줄 번호가 가챠 API 로 올 수 있다.
        public GachaScheduleProto GachaPrt
        {
            get
            {
                var num = Num;
                var scheduleType = Prt.Type;
                ReqHelper.ValidContext(scheduleType == EScheduleType.GACHA, "NOT_GACHA_SCHEDULE",
                    () => new { ScheduleNum = num, ScheduleType = scheduleType });
                return ProtoDb.Get<GachaScheduleProto>(num);
            }
        }

        public bool IsActivePeriod(DateTime nowTime) => TimeHelper.IsValidDateTime(nowTime, ActiveStartTime, ActiveEndTime);
        public bool IsContentPeriod(DateTime nowTime) => TimeHelper.IsValidDateTime(nowTime, ContentStartTime, ContentEndTime);
        public bool IsRewardPeriod(DateTime nowTime) => TimeHelper.IsValidDateTime(nowTime, ContentEndTime, ActiveEndTime);

        // ── 가챠. "이 스케줄로 몇 연차가 가능한가" 는 스케줄의 성질이라 여기 둔다 ──
        public int ValidGachaCnt(int reqCnt)
        {
            var num = Num;
            var prtGacha = GachaPrt;
            var findIdx = prtGacha.CntList.FindIndex(x => x == reqCnt);
            ReqHelper.ValidContext(findIdx != -1, "NOT_EQUAL_GACHA_CNT", () => new { ScheduleNum = num, ReqCnt = reqCnt });

            return prtGacha.CntList[findIdx];
        }

        public ObjValue ValidGachaCost(CostObjPacket reqCostObj, int valCnt)
        {
            var num = Num;
            var prtGacha = GachaPrt;
            var costIdx = prtGacha.CostTypeList.FindIndex(x => x == reqCostObj.Type);
            ReqHelper.ValidContext(costIdx != -1, "NOT_EQUAL_GACHA_COST_TYPE", () => new { ScheduleNum = num, ReqCostObj = reqCostObj });

            var valCostAmount = prtGacha.CostAmountList[costIdx] * valCnt;
            return ReqHelper.ValidCost(reqCostObj, prtGacha.CostTypeList[costIdx], 0, valCostAmount, MakeGachaReason(prtGacha, cnt: valCnt));
        }

        public string MakeGachaReason(int cnt)
        {
            return MakeGachaReason(GachaPrt, cnt);
        }

        // 프로토를 이미 들고 있는 호출부용. 메서드 하나가 ProtoDb 를 두 번 뒤지지 않게 한다.
        private string MakeGachaReason(GachaScheduleProto prtGacha, int cnt)
        {
            return $"GACHA:{Num}:{prtGacha.Tag}:{cnt}";
        }

        // 서버 시각은 호출부가 넣는다. 데이터 계층이 컨텍스트를 읽지 않게 하려는 것이다.
        public void ValidPeriod(EScheduleTimeType validTimeType, DateTime nowTime)
        {
            // 구조체라 람다가 this 를 못 잡는다. 지역으로 복사해서 넘긴다.
            var num = Num;
            switch (validTimeType)
            {
                case EScheduleTimeType.TOTAL:
                    ReqHelper.ValidContext(IsActivePeriod(nowTime), "NOT_ACTIVE_TOTAL_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.REWARD:
                    ReqHelper.ValidContext(IsRewardPeriod(nowTime), "NOT_ACTIVE_REWARD_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.CONTENT:
                    ReqHelper.ValidContext(IsContentPeriod(nowTime), "NOT_ACTIVE_CONTENT_TIME_SCHEDULE", () => new { Num = num });
                    break;
            }
        }
    }
}

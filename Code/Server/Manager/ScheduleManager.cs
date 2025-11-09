using Proto;
using Protocol;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public class ScheduleManager : CenterManagerBase<ScheduleModel>
    {
        public int Num => _prt.Num;
        public DateTime ActiveStartTime { get; private set; }
        public DateTime ActiveEndTime { get; private set; }
        public DateTime ContentStartTime { get; private set; }
        public DateTime ContentEndTime { get; private set; }
        public int State { get; private set; }
        public GachaScheduleProto GachaPrt => _prtGacha;

        private ScheduleProto _prt;
        private GachaScheduleProto _prtGacha;

        public ScheduleManager(CenterRepo centerRepo, ScheduleProto prt, ScheduleModel model = null) : base(centerRepo, model)
        {
            _prt = prt;

            switch (_prt.Type)
            {
                case EScheduleType.GACHA:
                    _prtGacha = APP.Prt.GetGachaSchedulePrt(_prt.Num); // <-------이거 변하지 않는 값이므로 Schedule관련 정보채로 캐싱해두는것 좋음
                    break;
            }

            ActiveStartTime = _prt.ActiveStartTime;
            ActiveEndTime = _prt.ActiveEndTime;
            ContentStartTime = _prt.ContentStartTime;
            ContentEndTime = _prt.ContentEndTime;

            if (model != null)
            {
                ActiveStartTime = model.ActiveStartTime;
                ActiveEndTime = model.ActiveEndTime;
                ContentStartTime = model.ContentStartTime;
                ContentEndTime = model.ContentEndTime;
            }
        }

   /*     public ScheduleManager(CenterRepo centerRepo, int scheduleNum, ScheduleModel model = null) : base(centerRepo, model)
        {
            _prt = APP.Prt.GetSchedulePrt(scheduleNum);
            ActiveStartTime = _prt.ActiveStartTime;
            ActiveEndTime = _prt.ActiveEndTime;
            ContentStartTime = _prt.ContentStartTime;
            ContentEndTime = _prt.ContentEndTime;

            if (model != null)
            {
                ActiveStartTime = model.ActiveStartTime;
                ActiveEndTime = model.ActiveEndTime;
                ContentStartTime = model.ContentStartTime;
                ContentEndTime = model.ContentEndTime;
            }
        }*/

        public bool IsActivePeriod(DateTime nowTime)
        {
            return TimeHelper.IsValidDateTime(nowTime, ActiveStartTime, ActiveEndTime);
        }

        public bool IsContentPeriod(DateTime nowTime)
        {
            return TimeHelper.IsValidDateTime(nowTime, ContentStartTime, ContentEndTime);
        }

        public bool IsRewardPeriod(DateTime nowTime)
        {
            return TimeHelper.IsValidDateTime(nowTime, ContentEndTime, ActiveEndTime);
        }

        #region GACHA
        public int ValidGachaCnt(int reqCnt)
        {
            var findIdx = _prtGacha.CntList.FindIndex(x => x == reqCnt);
            ReqHelper.ValidContext(findIdx != -1, "NOT_EQUAL_GACHA_CNT", () => new { ScheduleNum = _prt.Num, ReqCnt = reqCnt});

            return _prtGacha.CntList[findIdx];
        }

        public ObjValue ValidGachaCost(CostObjPacket reqCostObj, int valCnt)
        {
            var costIdx = _prtGacha.CostTypeList.FindIndex(x => x == reqCostObj.Type);
            ReqHelper.ValidContext(costIdx != -1, "NOT_EQUAL_GACHA_COST_TYPE", () => new { ScheduleNum = _prt.Num, ReqCostObj = reqCostObj });

            var prtCostType = _prtGacha.CostTypeList[costIdx];
            var prtCostAmount = _prtGacha.CostAmountList[costIdx];
            var valCostAmount = prtCostAmount * valCnt;

            var reason = MakeGachaReason(valCnt);
            var valCost = ReqHelper.ValidCost(reqCostObj, prtCostType, 0, valCostAmount, reason);
            return valCost;
        }

        public string MakeGachaReason(int cnt)
        {
            var reason = $"GACHA:{this.Num}:{_prtGacha.Tag}:{cnt}";
            return reason;
        }
        #endregion
    }
}

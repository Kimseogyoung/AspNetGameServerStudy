using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class ScheduleComponent : CenterComponentBase
    {
        public ScheduleComponent(CenterRepo centerRepo, IRepository repository) : base(centerRepo, repository)
        {
        }

        public List<ScheduleManager> GetList()
        {
            // 전체 조회 — 캐시 -> DB조회 일반화가 어려운 부분이라 DbSession 직접 사용
            var mdlList = DbSession.Execute(db => db.SelectListByConditions<ScheduleModel>(null).ToList());

            var prts = APP.Prt.GetSchedulePrts();
            var mgrList = new List<ScheduleManager>();
            foreach (var prt in prts)
            {
                var mdl = mdlList.FirstOrDefault(x => x.Num == prt.Num);
                var mgr = new ScheduleManager(_centerRepo, prt, mdl);
                mgrList.Add(mgr);
            }

            return mgrList;
        }

        public ScheduleManager Get(int num, EScheduleTimeType validTimeType = EScheduleTimeType.NONE)
        {
            ReqHelper.ValidContext(TryGet(num, out var mgrSchedule), "NOT_FOUND_SCHEDULE", () => new { Num = num });
            switch (validTimeType)
            {
                case EScheduleTimeType.TOTAL:
                    ReqHelper.ValidContext(mgrSchedule.IsActivePeriod(RpcCtx.ServerTime), "NOT_ACTIVE_TOTAL_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.REWARD:
                    ReqHelper.ValidContext(mgrSchedule.IsRewardPeriod(RpcCtx.ServerTime), "NOT_ACTIVE_REWARD_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.CONTENT:
                    ReqHelper.ValidContext(mgrSchedule.IsContentPeriod(RpcCtx.ServerTime), "NOT_ACTIVE_CONTENT_TIME_SCHEDULE", () => new { Num = num });
                    break;
            }
            return mgrSchedule;
        }

        public bool TryGet(int num, out ScheduleManager outSchedule)
        {
            var prt = APP.Prt.GetSchedulePrt(num);
            var mdlSchedule = GetMdl(db => db.SelectByPk<ScheduleModel>(new { Num = num }));
            outSchedule = new ScheduleManager(_centerRepo, prt, mdlSchedule);
            return mdlSchedule != null;
        }
    }
}

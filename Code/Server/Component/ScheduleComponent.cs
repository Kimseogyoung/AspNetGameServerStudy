using Proto;
using System.Security.Cryptography;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class ScheduleComponent : CenterComponentBase
    {
        public ScheduleComponent(CenterRepo centerRepo, DBSqlExecutor executor) : base(centerRepo, executor)
        {
        }

        public List<ScheduleManager> GetList()
        {
            var mdlList = new List<ScheduleModel>();

            // TODO: 자주 바뀌지 않으므로 Mgr 캐싱
            // 전부 로드
            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlList = sqlConnection.SelectListByConditions<ScheduleModel>(null, transaction).ToList();
            });

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
                    ReqHelper.ValidContext(mgrSchedule.IsActivePeriod(_centerRepo.RpcContext.ServerTime), "NOT_ACTIVE_TOTAL_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.REWARD:
                    ReqHelper.ValidContext(mgrSchedule.IsRewardPeriod(_centerRepo.RpcContext.ServerTime), "NOT_ACTIVE_REWARD_TIME_SCHEDULE", () => new { Num = num });
                    break;
                case EScheduleTimeType.CONTENT:
                    ReqHelper.ValidContext(mgrSchedule.IsContentPeriod(_centerRepo.RpcContext.ServerTime), "NOT_ACTIVE_CONTENT_TIME_SCHEDULE", () => new { Num = num });
                    break;
            }
            return mgrSchedule;
        }

        public bool TryGet(int num, out ScheduleManager outSchedule)
        {
            var prt = APP.Prt.GetSchedulePrt(num);
            ScheduleModel mdlSchedule = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlSchedule = sqlConnection.SelectByPk<ScheduleModel>(new { Num = num}, transaction);
            });

            outSchedule = new ScheduleManager(_centerRepo, prt, mdlSchedule);
            return outSchedule != null;
        }
    }
}

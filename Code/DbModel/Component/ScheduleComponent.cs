using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using ServerCore.Extension;
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

        public async Task<List<ScheduleManager>> GetListAsync()
        {
            // 전체 조회 — 캐시 -> DB조회 일반화가 어려운 부분이라 DbSession 직접 사용
            var mdlList = await DbSession.ExecuteAsync(async db => (await db.SelectListByConditions<ScheduleModel>(null)).ToList());

            var prts = ProtoDb.GetAll<ScheduleProto>();
            var mgrList = new List<ScheduleManager>();
            foreach (var prt in prts)
            {
                var mdl = mdlList.FirstOrDefault(x => x.Num == prt.Num);
                var mgr = new ScheduleManager(_centerRepo, prt, mdl);
                mgrList.Add(mgr);
            }

            return mgrList;
        }

        public async Task<ScheduleManager> GetAsync(int num, EScheduleTimeType validTimeType = EScheduleTimeType.NONE)
        {
            var (found, mgrSchedule) = await TryGetAsync(num);
            ReqHelper.ValidContext(found, "NOT_FOUND_SCHEDULE", () => new { Num = num });
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

        public async Task<(bool Found, ScheduleManager? Value)> TryGetAsync(int num)
        {
            var prt = ProtoDb.Get<ScheduleProto>(num);
            var mdlSchedule = await GetMdlAsync(db => db.SelectByPk<ScheduleModel>(new { Num = num }));
            return mdlSchedule == null ? (false, null) : (true, new ScheduleManager(_centerRepo, prt, mdlSchedule));
        }
    }
}

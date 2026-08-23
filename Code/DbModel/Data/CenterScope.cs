using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 센터 DB. 소유자 축이 없어서 경계가 아니라 DB 선택이다. 그래서 인자가 없고
    // OwnedSet<T> 도 못 쓴다(스코프 키가 없으면 캐시 키도 자동 WHERE 도 못 만든다).
    //
    // Center DB 는 캐시를 안 쓴다. 스케줄 전량 조회는 ScheduleLoad 한 곳뿐이고
    // 가챠는 PK 단건이라, 전역 캐시를 넣으면 무효화 설계가 이득보다 커진다.
    public class CenterScope
    {
        internal CenterScope(GameDb db)
        {
            _db = db;
        }

        // 프로토에 있는 스케줄 전부를 돌려주고, DB 행이 없는 것은 프로토 값으로 채운다.
        // 행이 있어야만 찾은 것으로 치는 TryGetScheduleAsync 와 기준이 다르므로 이름으로 구분한다.
        public async Task<List<ScheduleView>> GetFilledScheduleListAsync()
        {
            var mdlList = await Db.ExecuteAsync(async db => (await db.SelectListByConditionsAsync<ScheduleModel>(null)).ToList());

            var viewList = new List<ScheduleView>();
            foreach (var prt in ProtoDb.GetAll<ScheduleProto>())
            {
                viewList.Add(new ScheduleView(prt, mdlList.Find(x => x.Num == prt.Num)));
            }

            return viewList;
        }

        public async Task<(bool Found, ScheduleView Value)> TryGetScheduleAsync(int num)
        {
            var prt = ProtoDb.Get<ScheduleProto>(num);
            var mdlSchedule = await Db.ExecuteAsync(db => db.SelectByPkAsync<ScheduleModel>(new { Num = num }));
            return mdlSchedule == null ? (false, default) : (true, new ScheduleView(prt, mdlSchedule));
        }

        public async Task<ScheduleView> GetScheduleAsync(int num)
        {
            var (found, view) = await TryGetScheduleAsync(num);
            ReqHelper.ValidContext(found, "NOT_FOUND_SCHEDULE", () => new { Num = num });
            return view;
        }

        // Center DB 는 캐시를 안 쓰므로 IRepository 의 캐시 경로를 안 지난다.
        private IDbSession Db => _db.CenterRepository().Db;

        private readonly GameDb _db;
    }
}

using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class WorldQueries
    {
        public static async Task<WorldModel> GetOrCreateAsync(this OwnedSet<WorldModel> set, int num)
        {
            var (found, mdlWorld) = await set.TryGetAsync(x => x.Num == num);
            return found ? mdlWorld : await set.CreateAsync(new WorldModel { Num = num });
        }

        // 이전 월드를 깼는가. 첫 월드면 이전이 없으므로 항상 참이다.
        public static async Task<bool> IsFinishPrevWorldAsync(this OwnedSet<WorldModel> set, WorldProto prtWorld)
        {
            var prtPrevWorld = ProtoDb.GetByMk<WorldProto>(prtWorld.Type).LastOrDefault(x => x.Order < prtWorld.Order);
            if (prtPrevWorld == null)
            {
                return true;
            }

            var (found, mdlPrevWorld) = await set.TryGetAsync(x => x.Num == prtPrevWorld.Num);
            return found && mdlPrevWorld.IsFinish();
        }
    }
}

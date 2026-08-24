using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class WorldStageQueries
    {
        // 월드별 별 집계가 WorldNum 으로 거르므로 생성할 때 같이 넣는다.
        public static async Task<WorldStageModel> GetOrCreateAsync(this OwnedSet<WorldStageModel> set, int num, int worldNum)
        {
            var (found, mdlWorldStage) = await set.TryGetAsync(x => x.Num == num);
            if (found)
            {
                // WorldNum 이 0 인 행은 읽는 김에 메운다.
                if (mdlWorldStage.WorldNum == 0)
                {
                    mdlWorldStage.WorldNum = worldNum;
                    await set.UpdateAsync(mdlWorldStage);
                }

                return mdlWorldStage;
            }

            return await set.CreateAsync(new WorldStageModel { Num = num, WorldNum = worldNum });
        }

        // 월드당 스테이지가 10개 남짓이고 소유자 리스트는 이미 캐시에 있어 메모리에서 센다.
        public static async Task<int> GetTotalStarAsync(this OwnedSet<WorldStageModel> set, int worldNum)
        {
            var list = await set.GetListAsync();
            return list.Where(x => x.WorldNum == worldNum).Sum(x => x.Star);
        }
    }
}

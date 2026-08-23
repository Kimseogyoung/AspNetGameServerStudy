using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class WorldStageQueries
    {
        // WorldNum 을 같이 넣는다. 옛 경로는 안 넣어서 항상 0 이었고, 그 탓에 월드별 별 집계가
        // 한 행도 못 찾았다.
        public static async Task<WorldStageModel> GetOrCreateAsync(this OwnedSet<WorldStageModel> set, int num, int worldNum)
        {
            var (found, mdlWorldStage) = await set.TryGetAsync(x => x.Num == num);
            if (found)
            {
                // 옛 경로가 만든 행은 WorldNum 이 0 이다. 신규만 채우면 기존 유저는 영영 0 이라
                // 만나는 김에 메운다.
                if (mdlWorldStage.WorldNum == 0)
                {
                    mdlWorldStage.WorldNum = worldNum;
                    await set.UpdateAsync(mdlWorldStage);
                }

                return mdlWorldStage;
            }

            return await set.CreateAsync(new WorldStageModel { Num = num, WorldNum = worldNum });
        }

        // 월드당 스테이지가 10개 남짓이라 메모리에서 센다. 소유자 리스트는 이미 캐시에 있어
        // DB 로 다시 갈 이유가 없고, 집계 SQL 은 InMemory 모드에서 못 돌아 테스트도 안 됐다.
        public static async Task<int> GetTotalStarAsync(this OwnedSet<WorldStageModel> set, int worldNum)
        {
            var list = await set.GetListAsync();
            return list.Where(x => x.WorldNum == worldNum).Sum(x => x.Star);
        }
    }
}

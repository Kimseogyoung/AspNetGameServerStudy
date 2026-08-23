using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class PlayerDetailQueries
    {
        // 플레이어당 한 행이라 리스트의 첫 항목이 곧 그 플레이어의 것이다.
        public static async Task<PlayerDetailModel> GetOrCreateAsync(this OwnedSet<PlayerDetailModel> set)
        {
            var list = await set.GetListAsync();
            return list.Count > 0 ? list[0] : await set.CreateAsync(new PlayerDetailModel());
        }
    }
}

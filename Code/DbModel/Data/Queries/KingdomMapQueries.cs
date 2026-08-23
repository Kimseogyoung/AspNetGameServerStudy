using Proto;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class KingdomMapQueries
    {
        // 플레이어당 한 행이라 리스트의 첫 항목이 곧 그 플레이어의 것이다.
        public static async Task<(bool Found, KingdomMapModel Value)> TryGetAsync(this OwnedSet<KingdomMapModel> set)
        {
            var list = await set.GetListAsync();
            return list.Count > 0 ? (true, list[0]) : (false, null);
        }

        public static async Task<KingdomMapModel> GetOrCreateAsync(this OwnedSet<KingdomMapModel> set)
        {
            var (found, mdl) = await set.TryGetAsync();
            if (found)
            {
                return mdl;
            }

            var newMdl = new KingdomMapModel
            {
                Snapshot = "",
                State = EKingdomTileMapState.NONE,
            };
            return await set.CreateAsync(newMdl);
        }
    }
}

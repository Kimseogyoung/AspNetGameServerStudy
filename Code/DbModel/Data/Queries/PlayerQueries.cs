using ServerCore.Helper;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace WebStudyServer.Data.Queries
{
    public static class PlayerQueries
    {
        // PlayerModel 의 ScopeKey 는 Id 자신이라 스코프당 한 행이다.
        public static async Task<(bool Found, PlayerModel Value)> TryGetAsync(this OwnedSet<PlayerModel> set)
        {
            var list = await set.GetListAsync();
            return list.Count > 0 ? (true, list[0]) : (false, null);
        }

        // PlayerId 가 0 인지는 PlayerAuthPolicy 가 이미 막으므로 여기선 안 본다.
        public static async Task<PlayerModel> GetAsync(this OwnedSet<PlayerModel> set)
        {
            var (found, mdlPlayer) = await set.TryGetAsync();
            ReqHelper.ValidContext(found, "NOT_FOUND_PLAYER", () => new { PlayerId = set.ScopeKeyValue });
            return mdlPlayer;
        }

        // Id 는 스코프가 정한다(ScopeKey = "Id"). 여기서 넣어도 CreateAsync 가 덮으므로
        // 호출부는 스코프를 열기 전에 PlayerId 를 확정해야 한다.
        public static async Task<PlayerModel> GetOrCreateAsync(this OwnedSet<PlayerModel> set, ulong accountId)
        {
            var (found, mdlPlayer) = await set.TryGetAsync();
            if (found)
            {
                return mdlPlayer;
            }

            return await set.CreateAsync(new PlayerModel
            {
                AccountId = accountId,
                SfId = IdHelper.GenerateSfId(),
                ProfileName = "",
            });
        }
    }
}

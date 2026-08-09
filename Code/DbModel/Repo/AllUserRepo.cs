using WebStudyServer.Model;
using ServerCore.Repo.Database;

namespace WebStudyServer.Repo
{
    public class AllUserRepo
    {
        private readonly List<IDbSession> _factories;

        // 우선 DB전용.. Repo로 구현하였으나 개선 필요.
        public AllUserRepo(List<IDbSession> factories)
        {
            _factories = factories;
        }

        public async Task<(bool Found, PlayerModel? Value)> TryGetPlayerByNameAsync(string name)
        {
            // TODO: 캐시

            // 샤드 전체 탐색
            foreach (var factory in _factories)
            {
                var mdlPlayer = await factory.ExecuteAsync(db => db.SelectByConditions<PlayerModel>(new { ProfileName = name }));
                if (mdlPlayer != null)
                {
                    return (true, mdlPlayer);
                }
            }

            return (false, null);
        }
    }
}

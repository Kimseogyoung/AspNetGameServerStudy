using ServerCore.Repo.Database;
using WebStudyServer.Model;

namespace WebStudyServer.Data
{
    // 소유자를 모르는 전 샤드 조회. UserScope는 단일 샤드 전제라 스코프 밖에 둔다.
    // 샤드를 하나씩 열며 찾고, 캐시는 쓰지 않는다.
    public class AllShards
    {
        internal AllShards(GameDb db)
        {
            _db = db;
        }

        public async Task<(bool Found, PlayerModel Value)> TryGetPlayerByNameAsync(string profileName)
        {
            foreach (var connectionString in DbConnectionResolver.AllUsers())
            {
                var player = await _db.SessionFor(connectionString)
                    .ExecuteAsync(db => db.SelectByConditionsAsync<PlayerModel>(new { ProfileName = profileName }));

                if (player != null)
                {
                    return (true, player);
                }
            }

            return (false, null);
        }

        private readonly GameDb _db;
    }
}

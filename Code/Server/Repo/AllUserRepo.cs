using WebStudyServer.Model;
using WebStudyServer.Repo.Database;

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

        public bool TryGetPlayerByName(string name, out PlayerModel outMdlPlayer)
        {
            // TODO: 캐시
            //

            // 샤드 전체 탐색
            PlayerModel foundMdlPlayer = null;
            foreach (var factory in _factories)
            {
                factory.Execute(db =>
                {
                    var mdlPlayer = db.SelectByConditions<PlayerModel>(new { ProfileName = name });
                    if (mdlPlayer != null)
                    {
                        foundMdlPlayer = mdlPlayer;
                    }
                });

                if (foundMdlPlayer != null)
                {
                    break;
                }
            }

            outMdlPlayer = foundMdlPlayer;
            return outMdlPlayer != null;
        }
    }
}

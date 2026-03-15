using Proto;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class PlayerMapComponent : AuthComponentBase
    {
        public PlayerMapComponent(AuthRepo authRepo, IDbSession dbFactory) : base(authRepo, dbFactory)
        {
        }

        public PlayerMapModel Create(PlayerMapModel inPlayerMap)
        {
            PlayerMapModel newPlayerMap = null;
            // 데이터베이스에 삽입
            _dbFactory.Execute(db =>
            {
                newPlayerMap = db.Insert(inPlayerMap);
            });

            return newPlayerMap;
        }

        public bool TryGetPlayerMap(ulong accountId, out PlayerMapModel outPlayerMap)
        {
            PlayerMapModel playerMap = null;

            _dbFactory.Execute(db =>
            {
                playerMap = db.SelectByPk<PlayerMapModel>(new { AccountId = accountId });
            });

            outPlayerMap = playerMap;
            return outPlayerMap != null;
        }
    }
}

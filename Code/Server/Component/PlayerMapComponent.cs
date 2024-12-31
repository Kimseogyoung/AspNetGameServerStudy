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
        public PlayerMapComponent(AuthRepo authRepo, DBSqlExecutor executor) : base(authRepo, executor)
        {
        }

        public PlayerMapModel Create(PlayerMapModel inPlayerMap)
        {
            PlayerMapModel newPlayerMap = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newPlayerMap = sqlConnection.Insert(inPlayerMap, transaction);
            });

            return newPlayerMap;
        }

        public bool TryGetPlayerMap(ulong accountId, out PlayerMapModel outPlayerMap)
        {
            PlayerMapModel playerMap = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                playerMap = sqlConnection.SelectByPk<PlayerMapModel>(new { AccountId = accountId }, transaction);
            });

            outPlayerMap = playerMap;
            return outPlayerMap != null;
        }
    }
}

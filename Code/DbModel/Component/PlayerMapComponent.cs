using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using ServerCore.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Component
{
    public class PlayerMapComponent : AuthComponentBase
    {
        public PlayerMapComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public PlayerMapModel Create(PlayerMapModel inPlayerMap)
        {
            return CreateMdl(inPlayerMap);
        }

        public bool TryGetPlayerMap(ulong accountId, out PlayerMapModel outPlayerMap)
        {
            outPlayerMap = GetMdl(db => db.SelectByPk<PlayerMapModel>(new { AccountId = accountId }));
            return outPlayerMap != null;
        }
    }
}

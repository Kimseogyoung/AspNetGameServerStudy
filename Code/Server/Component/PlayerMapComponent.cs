using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Component
{
    public class PlayerMapComponent : AuthComponentBase
    {
        public static class Key
        {
            public static CacheKey ByAccountId(ulong accountId) => CacheKey.For<PlayerMapModel>(accountId);
        }

        public PlayerMapComponent(AuthRepo authRepo, IRepository repository) : base(authRepo, repository)
        {
        }

        public PlayerMapModel Create(PlayerMapModel inPlayerMap)
        {
            return CreateMdl(inPlayerMap, e => Key.ByAccountId(e.AccountId));
        }

        public bool TryGetPlayerMap(ulong accountId, out PlayerMapModel outPlayerMap)
        {
            outPlayerMap = GetMdl(Key.ByAccountId(accountId), db => db.SelectByPk<PlayerMapModel>(new { AccountId = accountId }));
            return outPlayerMap != null;
        }
    }
}

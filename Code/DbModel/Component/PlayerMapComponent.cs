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

        public Task<PlayerMapModel> CreateAsync(PlayerMapModel inPlayerMap)
        {
            return CreateMdlAsync(inPlayerMap);
        }

        public async Task<(bool Found, PlayerMapModel? Value)> TryGetPlayerMapAsync(ulong accountId)
        {
            var mdlPlayerMap = await GetMdlAsync(db => db.SelectByPk<PlayerMapModel>(new { AccountId = accountId }));
            return (mdlPlayerMap != null, mdlPlayerMap);
        }
    }
}

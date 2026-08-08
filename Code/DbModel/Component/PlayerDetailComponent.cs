using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PlayerDetailComponent : UserComponentBase<PlayerDetailModel>
    {
        public PlayerDetailComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(PlayerDetailModel model) => CacheKey.For(CacheKeyTags.PlayerDetailModel, model.PlayerId);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.PlayerDetailModel, playerId);

        public async Task<PlayerDetailManager> TouchAsync()
        {
            var playerId = _userRepo.RpcContext.PlayerId;

            var mdlPlayerDetail = await TryGetAsync(playerId);
            if (mdlPlayerDetail == null)
            {
                mdlPlayerDetail = await CreateMdlAsync(new PlayerDetailModel
                {
                    PlayerId = playerId,
                });
            }

            return new PlayerDetailManager(_userRepo, mdlPlayerDetail);
        }

        public Task<PlayerDetailModel?> TryGetAsync(ulong id)
        {
            return GetMdlAsync(x => x.PlayerId == id);
        }
    }
}

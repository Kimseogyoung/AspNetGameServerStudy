using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomMapComponent : UserComponentBase<KingdomMapModel>
    {
        public KingdomMapComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(KingdomMapModel model) => CacheKey.For(CacheKeyTags.KingdomMapModel, model.PlayerId);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.KingdomMapModel, playerId);

        public async Task<KingdomMapManager> TouchAsync()
        {
            var mdlKingdomMap = await TryGetInternalAsync();
            if (mdlKingdomMap == null)
            {
                mdlKingdomMap = await CreateMdlAsync(new KingdomMapModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Snapshot = "",
                    State = EKingdomTileMapState.NONE,
                });
            }

            return new KingdomMapManager(_userRepo, mdlKingdomMap);
        }

        private Task<KingdomMapModel?> TryGetInternalAsync()
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId);
        }
    }
}

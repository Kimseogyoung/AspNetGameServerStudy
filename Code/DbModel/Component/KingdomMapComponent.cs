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

        protected override CacheKey KeyFor(KingdomMapModel model) => CacheKey.For<KingdomMapModel>(model.PlayerId);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<KingdomMapModel>(playerId);

        public KingdomMapManager Touch()
        {
            if (!TryGetInternal(out var mdlKingdomMap))
            {
                mdlKingdomMap = CreateMdl(new KingdomMapModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Snapshot = "",
                    State = EKingdomTileMapState.NONE,
                });
            }

            return new KingdomMapManager(_userRepo, mdlKingdomMap);
        }

        private bool TryGetInternal(out KingdomMapModel outKingdomMap)
        {
            outKingdomMap = GetMdl(x => x.PlayerId == RpcCtx.PlayerId);
            return outKingdomMap != null;
        }
    }
}

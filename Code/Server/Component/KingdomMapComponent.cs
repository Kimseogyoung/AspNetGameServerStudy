using Proto;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class KingdomMapComponent : UserComponentBase<KingdomMapModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId) => CacheKey.For<KingdomMapModel>(playerId, playerId);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<KingdomMapModel>(playerId);
        }

        public KingdomMapComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(KingdomMapModel model) => Key.Single(model.PlayerId);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
            outKingdomMap = GetMdl(
                Key.Single(_rpcContext.PlayerId),
                db => db.SelectByPk<KingdomMapModel>(new { PlayerId = _rpcContext.PlayerId }));
            return outKingdomMap != null;
        }
    }
}

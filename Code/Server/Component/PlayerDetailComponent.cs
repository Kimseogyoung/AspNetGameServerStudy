using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PlayerDetailComponent : UserComponentBase<PlayerDetailModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId) => CacheKey.For<PlayerDetailModel>(playerId);
            public static CacheKey List(ulong playerId) => CacheKey.For<PlayerDetailModel>(playerId);
        }

        public PlayerDetailComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(PlayerDetailModel model) => Key.Single(model.PlayerId);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

        public PlayerDetailManager Touch()
        {
            var playerId = _userRepo.RpcContext.PlayerId;

            if (!TryGet(playerId, out var mdlPlayerDetail))
            {
                mdlPlayerDetail = CreateMdl(new PlayerDetailModel
                {
                    PlayerId = playerId,
                });
            }

            return new PlayerDetailManager(_userRepo, mdlPlayerDetail);
        }

        public bool TryGet(ulong id, out PlayerDetailModel outPlayer)
        {
            outPlayer = GetMdl(
                Key.Single(id),
                db => db.SelectByPk<PlayerDetailModel>(new { PlayerId = id }));
            return outPlayer != null;
        }
    }
}

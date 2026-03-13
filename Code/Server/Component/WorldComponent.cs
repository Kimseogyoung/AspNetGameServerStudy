using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class WorldComponent : UserComponentBase<WorldModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<WorldModel>(playerId, playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<WorldModel>(playerId);
        }

        public WorldComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(WorldModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

        public WorldManager Touch(int worldNum)
        {
            if (!TryGetInternal(worldNum, out var mdlWorld))
            {
                mdlWorld = CreateMdl(new WorldModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = worldNum,
                });
            }

            return new WorldManager(_userRepo, mdlWorld);
        }

        public bool TryGetInternal(int num, out WorldModel outWorld)
        {
            outWorld = GetMdl(
                Key.Single(_rpcContext.PlayerId, num),
                db => db.SelectByPk<WorldModel>(new { PlayerId = _rpcContext.PlayerId, Num = num }));
            return outWorld != null;
        }
    }
}

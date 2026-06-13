using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class WorldComponent : UserComponentBase<WorldModel>
    {
        public WorldComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(WorldModel model) => CacheKey.For<WorldModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<WorldModel>(playerId);

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
            outWorld = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outWorld != null;
        }
    }
}

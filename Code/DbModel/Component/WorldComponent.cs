using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class WorldComponent : UserComponentBase<WorldModel>
    {
        public WorldComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(WorldModel model) => CacheKey.For(CacheKeyTags.WorldModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.WorldModel, playerId);

        public async Task<WorldManager> TouchAsync(int worldNum)
        {
            var mdlWorld = await TryGetInternalAsync(worldNum);
            if (mdlWorld == null)
            {
                mdlWorld = await CreateMdlAsync(new WorldModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = worldNum,
                });
            }

            return new WorldManager(_userRepo, mdlWorld);
        }

        public Task<WorldModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}

using Dapper;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class WorldStageComponent : UserComponentBase<WorldStageModel>
    {
        public WorldStageComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(WorldStageModel model) => CacheKey.For(CacheKeyTags.WorldStageModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.WorldStageModel, playerId);

        public async Task<WorldStageManager> TouchAsync(int worldStageNum)
        {
            var mdlWorldStage = await TryGetInternalAsync(worldStageNum);
            if (mdlWorldStage == null)
            {
                mdlWorldStage = await CreateMdlAsync(new WorldStageModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = worldStageNum,
                });
            }

            return new WorldStageManager(_userRepo, mdlWorldStage);
        }

        public int GetTotalStar(int worldNum)
        {
            // TODO: 캐시
            var sql = "SELECT SUM(RewardAmount) FROM WorldStage WHERE PlayerId = @PlayerId AND WorldNum = @WorldNum";
            return DbSession.Execute(db => db.QuerySingle<int>(sql,
                new { RpcCtx.PlayerId, WorldNum = worldNum }));
        }

        public Task<WorldStageModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}

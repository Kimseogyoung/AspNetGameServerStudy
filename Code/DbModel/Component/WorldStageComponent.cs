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

        protected override CacheKey KeyFor(WorldStageModel model) => CacheKey.For<WorldStageModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<WorldStageModel>(playerId);

        public WorldStageManager Touch(int worldStageNum)
        {
            if (!TryGetInternal(worldStageNum, out var mdlWorldStage))
            {
                mdlWorldStage = CreateMdl(new WorldStageModel
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

        public bool TryGetInternal(int num, out WorldStageModel outWorldStage)
        {
            outWorldStage = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outWorldStage != null;
        }
    }
}

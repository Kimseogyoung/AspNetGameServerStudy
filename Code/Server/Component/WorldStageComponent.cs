using Dapper;
using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class WorldStageComponent : UserComponentBase<WorldStageModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<WorldStageModel>(playerId, playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<WorldStageModel>(playerId);
        }

        public WorldStageComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(WorldStageModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
            return DbFactory.Execute(db => db.QuerySingle<int>(sql,
                new { RpcCtx.PlayerId, WorldNum = worldNum }));
        }

        public bool TryGetInternal(int num, out WorldStageModel outWorldStage)
        {
            outWorldStage = GetMdl(
                Key.Single(RpcCtx.PlayerId, num),
                db => db.SelectByPk<WorldStageModel>(new { RpcCtx.PlayerId, Num = num }));
            return outWorldStage != null;
        }
    }
}

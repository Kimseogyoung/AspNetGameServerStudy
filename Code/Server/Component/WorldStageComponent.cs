using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Dapper;

namespace WebStudyServer.Component
{
    public class WorldStageComponent : UserComponentBase<WorldStageModel>
    {
        public WorldStageComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {
        }

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

            var mgrWorldStage = new WorldStageManager(_userRepo, mdlWorldStage);
            return mgrWorldStage;
        }

        public int GetTotalStar(int worldNum)
        {
            var totalStar = 0;
            // TODO: 캐시
            _executor.Excute((sqlConnection, transaction) =>
            {
                var sql = "SELECT SUM(RewardAmount) FROM WorldStage WHERE PlayerId = @PlayerId AND WorldNum = @WorldNum";
                totalStar = sqlConnection.QuerySingleOrDefault<int>(sql, new { PlayerId = _userRepo.RpcContext.PlayerId, WorldNum = worldNum });
            });

            return totalStar;
        }

        public bool TryGetInternal(int num, out WorldStageModel outWorldStage)
        {
            WorldStageModel mdlWorldStage = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlWorldStage = sqlConnection.SelectByPk<WorldStageModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outWorldStage = mdlWorldStage;
            return outWorldStage != null;
        }
    }
}

using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;
using WebStudyServer.GAME;

namespace WebStudyServer.Component
{
    public class WorldComponent : UserComponentBase<WorldModel>
    {
        public WorldComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {
        }

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

            var mgrWorld = new WorldManager(_userRepo, mdlWorld);
            return mgrWorld;
        }

        public bool TryGetInternal(int num, out WorldModel outWorld)
        {
            WorldModel mdlWorld = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlWorld = sqlConnection.SelectByPk<WorldModel>(new { PlayerId = _userRepo.RpcContext.PlayerId, Num = num }, transaction);
            });

            outWorld = mdlWorld;
            return outWorld != null;
        }
    }
}

using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;

namespace WebStudyServer.Component
{
    public class PlayerDetailComponent : UserComponentBase<PlayerDetailModel>
    {
        public PlayerDetailComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

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

            var mgrPlayerDetail = new PlayerDetailManager(_userRepo, mdlPlayerDetail);
            return mgrPlayerDetail;
        }

        public bool TryGet(ulong id, out PlayerDetailModel outPlayer)
        {
            PlayerDetailModel mdlPlayer = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlayer = sqlConnection.SelectByPk<PlayerDetailModel>(new { PlayerId = id }, transaction);
            });

            outPlayer = mdlPlayer;
            return outPlayer != null;
        }
    }
}

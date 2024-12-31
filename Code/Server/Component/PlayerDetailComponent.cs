using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;
using WebStudyServer.Extension;

namespace WebStudyServer.Component
{
    public class PlayerDetailComponent : UserComponentBase
    {
        public PlayerDetailComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {

        }

        public PlayerDetailManager Touch()
        {
            var playerId = _userRepo.RpcContext.PlayerId;

            if (!TryGet(playerId, out var mdlPlayerDetail))
            {
                mdlPlayerDetail = Create(new PlayerDetailModel
                {
                    PlayerId = playerId,
                });
            }

            var mgrPlayerDetail = new PlayerDetailManager(_userRepo, mdlPlayerDetail);
            return mgrPlayerDetail;
        }

        public PlayerDetailModel Create(PlayerDetailModel newPlayer)
        {
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newPlayer = sqlConnection.Insert<PlayerDetailModel>(newPlayer, transaction);
            });

            return newPlayer; // 새로 생성된 플레이어 모델 반환
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

        public void Update(PlayerDetailModel mdlPlayer)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlPlayer, transaction);
            });
        }
    }
}

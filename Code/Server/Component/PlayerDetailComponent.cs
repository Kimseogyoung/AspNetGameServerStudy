using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Helper;

namespace WebStudyServer.Component
{
    public class PlayerDetailComponent : UserComponentBase
    {
        public PlayerDetailComponent(UserRepo userRepo) : base(userRepo)
        {
        }

        public PlayerDetailManager TouchPlayerDetail()
        {
            var playerId = _userRepo.RpcContext.PlayerId;

            if (!_userRepo.TryGetPlayerDetail(playerId, out var mdlPlayerDetail))
            {
                mdlPlayerDetail = _userRepo.CreatePlayerDetail(new PlayerDetailModel
                {
                    PlayerId = playerId,
                });
            }

            var mgrPlayerDetail = new PlayerDetailManager(_userRepo, mdlPlayerDetail);
            return mgrPlayerDetail;
        }
    }
}

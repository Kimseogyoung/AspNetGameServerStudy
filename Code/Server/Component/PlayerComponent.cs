using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Helper;

namespace WebStudyServer.Component
{
    public class PlayerComponent : UserComponentBase
    {
        public PlayerComponent(AuthRepo authRepo, UserRepo userRepo) : base(userRepo)
        {
            _authRepo = authRepo;
            _authRepo.Init(0); // TODO: 개선
        }

        public PlayerManager TouchPlayer()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            var accountId = _userRepo.RpcContext.AccountId;

            PlayerModel mdlPlayer = null;
            if (playerId == 0)
            {
                // 신규 플레이어 생성
                mdlPlayer = _userRepo.CreatePlayer(new PlayerModel
                {
                    AccountId = accountId,
                    Lv = 1,
                    ProfileName = IdHelper.GenerateRandomName(), // TODO: 중복 닉네임 제한 하고싶음.
                });
                _userRepo.RpcContext.SetPlayerId(mdlPlayer.Id);
            }
            else
            {
                // 기존 플레이어 로드
                var mgrPlayer = GetPlayer();
                return mgrPlayer;
            }

            if (mdlPlayer == null)
                throw new Exception("NOT_FOUND_PLAYER"); // TODO:  오류 발생

            if (!_authRepo.TryGetPlayerMap(accountId, out _))
            {
                _authRepo.CreatePlayerMap(new PlayerMapModel
                {
                    AccountId = accountId,
                    PlayerId = mdlPlayer.Id,
                    ShardId = _userRepo.ShardId,
                });
                _authRepo.Commit(); // TODO: 개선
            }

            var newMgrPlayer = new PlayerManager(_userRepo, mdlPlayer);
            return newMgrPlayer;
        }

        public PlayerManager GetPlayer()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            ReqHelper.ValidContext(playerId != 0, "ZERO_PLAYER_ID", () => new { PlayerId = playerId });

            ReqHelper.ValidContext(_userRepo.TryGetPlayer(playerId, out var outMdlPlayer), "NOT_FOUND_PLAYER", ()=> new {PlayerId = playerId});
            var mgrPlayer = new PlayerManager(_userRepo, outMdlPlayer);
            return mgrPlayer;
        }

        private AuthRepo _authRepo;
    }
}

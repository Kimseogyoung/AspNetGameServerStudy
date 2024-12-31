using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;
using WebStudyServer.Model;

namespace Server.Service
{
    public class GameService : ServiceBase
    {
        public GameService(AllUserRepo allUserRepo, AuthRepo authRepo, UserRepo userRepo, RpcContext rpcContext, ILogger<GameService> logger) : base(rpcContext, logger)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
            _allUserRepo = allUserRepo;
        }

        #region GAME
        public GameEnterResult Enter()
        {
            var mgrPlayer = _userRepo.Player.Touch();

            if (mgrPlayer.Model.State >= Proto.EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                // 
            }
            else
            {
                mgrPlayer.PreparePlayer();

                var accountId = mgrPlayer.Model.AccountId;
                _authRepo.PlayerMap.Create(new PlayerMapModel
                {
                    AccountId = accountId,
                    PlayerId = mgrPlayer.Id,
                    ShardId = _userRepo.ShardId,
                });

                if (_authRepo.Session.TryGetByAccountId(accountId, out var mdlSession))
                {
                    mdlSession.SetPlayerId(mgrPlayer.Id);
                }

                _authRepo.Commit(); // TODO: 개선
            }

            return new GameEnterResult { Player = mgrPlayer.Model };
        }

        public string ChangeNameFirst(string name)
        {
            var mgrPlayer = _userRepo.Player.Touch();

            mgrPlayer.ValidState(EPlayerState.CHANGED_NAME_FIRST);

            // 중복 체크 (클라에 팝업)
            ReqHelper.Valid(!_allUserRepo.TryGetPlayerByName(name, out _), EErrorCode.GAME_CHANGE_NAME_EXIST_NAME);

            // 변경
            mgrPlayer.ChangeName(name);

            return mgrPlayer.Model.ProfileName;
        }
        #endregion

        #region KINGDOM_ITEM
        public KingdomItemBuyResPacket KingdomItemBuy(int reqKingdomItemNum, CostObjPacket costObj)
        {
            var mgrPlayer = _userRepo.Player.Touch();
            
            return new KingdomItemBuyResPacket {};
        }

        public KingdomItemConstructResPacket KingdomItemConstruct()
        {
            return new KingdomItemConstructResPacket { };
        }

        public KingdomItemCancelResPacket KingdomItemCancel()
        {
            return new KingdomItemCancelResPacket { };
        }

        public KingdomItemDecTimeResPacket KingdomItemDecTime()
        {
            return new KingdomItemDecTimeResPacket { };
        }
        #endregion

        private readonly AuthRepo _authRepo;
        private readonly UserRepo _userRepo;
        private readonly AllUserRepo _allUserRepo;
    }
}

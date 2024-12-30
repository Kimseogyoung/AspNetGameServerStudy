using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;

namespace Server.Service
{
    public class GameService : ServiceBase
    {
        public GameService(AllUserRepo allUserRepo, UserComponent userComp, RpcContext rpcContext, ILogger<GameService> logger) : base(rpcContext, logger)
        {
            _userComp = userComp;
            _allUserRepo = allUserRepo;
        }

        #region GAME
        public GameEnterResult Enter()
        {
            var mgrPlayer = _userComp.Player.TouchPlayer();

            if (mgrPlayer.Model.State >= Proto.EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                // 
            }
            else
            {
                mgrPlayer.PreparePlayer();
            }

            return new GameEnterResult { Player = mgrPlayer.Model };
        }

        public string ChangeNameFirst(string name)
        {
            var mgrPlayer = _userComp.Player.TouchPlayer();

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
            var mgrPlayer = _userComp.Player.TouchPlayer();
            
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

        private readonly UserComponent _userComp;
        private readonly AllUserRepo _allUserRepo;
    }
}

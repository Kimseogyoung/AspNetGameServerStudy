using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;
using WebStudyServer.Model;
using WebStudyServer.GAME;

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
                _authRepo.Init(0);
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
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqKingdomItemNum);

            // TODO: Item 최대 보유량 체크
            //

            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var resultCostObj = mgrPlayerDetail.DecCost(costObj, $"BUY_KINGDOM_ITEM:{reqKingdomItemNum}");

            var mgrKingdomItem = _userRepo.KingdomItem.Create(prtKingdomItem);
            return new KingdomItemBuyResPacket {};
        }

        public KingdomItemConstructResPacket KingdomItemConstruct(ulong kingdomItemId, int startTileX, int startTileY, int endTileX, int endTileY)
        {
            var mgrKingdomItem = _userRepo.KingdomItem.Get(kingdomItemId);

            // TODO: Tile 위치 중복 체크
            //

            mgrKingdomItem.Construct(startTileX, startTileY, endTileX, endTileY);
            return new KingdomItemConstructResPacket { };
        }

        public KingdomItemCancelResPacket KingdomItemCancel(ulong kingdomItemId)
        {
            var mgrKingdomItem = _userRepo.KingdomItem.Get(kingdomItemId);

            mgrKingdomItem.Cancel();
            return new KingdomItemCancelResPacket { };
        }

        public KingdomItemDecTimeResPacket KingdomItemDecTime(ulong kingdomItemId, int remainSec, CostCashPacket reqCostCash)
        {
            var mgrKingdomItem = _userRepo.KingdomItem.Get(kingdomItemId);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();

            // TODO: 남은 시간, 캐시 보유량 일치하는지 검증
            //

            var cashAmount = mgrPlayerDetail.DecCash(reqCostCash.Amount, $"DEC_TIME_KINGDOM_ITEM:{kingdomItemId}");
            mgrKingdomItem.DecTime();
            return new KingdomItemDecTimeResPacket { };
        }
        #endregion

        private readonly AuthRepo _authRepo;
        private readonly UserRepo _userRepo;
        private readonly AllUserRepo _allUserRepo;
    }
}

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
        public KingdomBuyStructureResPacket KingdomStructureBuy(int reqKingdomItemNum, CostObjPacket reqCostObj)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqKingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var hasItemCnt = _userRepo.KingdomStructure.GetKingdomStructureCnt(prtKingdomItem.Num);
            ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT", 
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_STRUCTURE:{reqKingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(reqCostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            var mgrKingdomStructure = _userRepo.KingdomStructure.Create(prtKingdomItem);
            return new KingdomBuyStructureResPacket { };
        }

        public KingdomBuyDecoResPacket KingdomDecoBuy(int reqKingdomItemNum, CostObjPacket reqCostObj)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(reqKingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var mgrKingdomDeco = _userRepo.KingdomDeco.Touch(prtKingdomItem.Num);
            ReqHelper.ValidContext(mgrKingdomDeco.Model.TotalCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_DECO_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = mgrKingdomDeco.Model.TotalCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_DECO:{reqKingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(reqCostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var chgCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);
            mgrKingdomDeco.Inc(1, reason);
            return new KingdomBuyDecoResPacket { };
        }

        public KingdomConstructStructureResPacket KingdomConstructStructure(ulong reqKingdomStructureId, CostObjPacket reqConstructCostObj, TilePosPacket reqStartTilePos, TilePosPacket reqEndTilePos)
        {
            var mgrKingdomStructure = _userRepo.KingdomStructure.Get(reqKingdomStructureId);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();

            // TODO: Tile 위치 중복 체크
            _userRepo.PlacedKingdomItem.ValidEmptyTile(reqStartTilePos, reqEndTilePos);

            // Cost일치하는지 체크
            var reason = $"CONSTURCT_KINGDOM_STRUCTURE:{reqKingdomStructureId}";
            var prtKingdomItem = mgrKingdomStructure.Prt;
            var valCostObj = ReqHelper.ValidCost(reqConstructCostObj, prtKingdomItem.ConstructObjType, prtKingdomItem.ConstructObjNum, prtKingdomItem.ConstructObjAmount, reason);

            // 처리
            // 처리: 건설 재료 소모
            var chgCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            // 처리: 타일 설치
            var placedKingdomItem = _userRepo.PlacedKingdomItem.Create(mgrKingdomStructure.Prt, reqStartTilePos.X, reqStartTilePos.Y, mgrKingdomStructure);

            // 처리: 건설 완료(상태 변경)
            mgrKingdomStructure.Construct();
            return new KingdomConstructStructureResPacket { };
        }

        public KingdomConstructDecoResPacket KingdomConstructDeco(int reqKingdomItemNum, TilePosPacket reqStartTilePos, TilePosPacket reqEndTilePos)
        {
            var mgrKingdomDeco = _userRepo.KingdomDeco.Touch(reqKingdomItemNum);

            // TODO: Tile 위치 중복 체크
            _userRepo.PlacedKingdomItem.ValidEmptyTile(reqStartTilePos, reqEndTilePos);

            // 처리 (즉시 설치)
            // 처리: 타일 설치
            var placedKingdomItem = _userRepo.PlacedKingdomItem.Create(mgrKingdomDeco.Prt, reqStartTilePos.X, reqStartTilePos.Y);

            // 처리: 건설 완료 (보유 개수 차감)
            mgrKingdomDeco.Construct();
            return new KingdomConstructDecoResPacket { };
        }

        public void KingdomFinishConstructStructure(ulong reqKingdomStructureId)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(reqKingdomStructureId);
            mgrKingdomItem.FinishConstruct();
        }

        public KingdomStoreResPacket KingdomItemCancel(ulong kingdomItemId)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(kingdomItemId);

            mgrKingdomItem.Cancel();
            return new KingdomStoreResPacket { };
        }

        public KingdomDecTimeStructureResPacket KingdomItemDecTime(ulong kingdomItemId, int remainSec, CostCashPacket reqCostCash)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(kingdomItemId);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();

            // TODO: 남은 시간, 캐시 보유량 일치하는지 검증
            //

            var cashAmount = mgrPlayerDetail.DecCash(reqCostCash.Amount, $"DEC_TIME_KINGDOM_ITEM:{kingdomItemId}");
            mgrKingdomItem.DecTime();
            return new KingdomDecTimeStructureResPacket { };
        }
        #endregion

        private readonly AuthRepo _authRepo;
        private readonly UserRepo _userRepo;
        private readonly AllUserRepo _allUserRepo;
    }
}

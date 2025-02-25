using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;
using WebStudyServer.Model;
using WebStudyServer.GAME;
using AutoMapper;

namespace Server.Service
{
    public class GameService : ServiceBase
    {
        public GameService(AllUserRepo allUserRepo, AuthRepo authRepo, UserRepo userRepo, CenterRepo centerRepo, IMapper mapper, RpcContext rpcContext, ILogger<GameService> logger) : base(rpcContext, logger)
        {
            _authRepo = authRepo;
            _userRepo = userRepo;
            _centerRepo = centerRepo;
            _allUserRepo = allUserRepo;
            _mapper = mapper;
        }

        #region GAME
        public GameEnterResPacket Enter(GameEnterReqPacket req)
        {
            var mgrPlayer = _userRepo.Player.Touch();

            if (mgrPlayer.Model.State >= Proto.EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                var pakPlayer = mgrPlayer.LoadPlayer(_mapper);
                return new GameEnterResPacket
                {
                    Player = pakPlayer,
                };
            }
            else
            {
                var pakPlayer = mgrPlayer.PreparePlayer(_mapper);

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

                return new GameEnterResPacket
                {
                    Player = pakPlayer,
                };
            }
        }

        public GameChangeNameResPacket ChangeNameFirst(GameChangeNameReqPacket req)
        {
            var reqName = req.PlayerName;
            var mgrPlayer = _userRepo.Player.Touch();

            mgrPlayer.ValidState(EPlayerState.CHANGED_NAME_FIRST);

            // 중복 체크 (클라에 팝업)
            ReqHelper.Valid(!_allUserRepo.TryGetPlayerByName(reqName, out _), EErrorCode.GAME_CHANGE_NAME_EXIST_NAME);

            // 변경
            mgrPlayer.ChangeName(reqName);

            return new GameChangeNameResPacket 
            {
                PlayerName = mgrPlayer.Model.ProfileName,
            };
        }
        #endregion

        #region KINGDOM
        public KingdomBuyStructureResPacket KingdomStructureBuy(KingdomBuyStructureReqPacket req)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var hasItemCnt = _userRepo.KingdomStructure.GetKingdomStructureCnt(prtKingdomItem.Num);
            ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT", 
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_STRUCTURE:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            var mgrKingdomStructure = _userRepo.KingdomStructure.Create(prtKingdomItem);
            return new KingdomBuyStructureResPacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                ChgObj = resultCostObj,
            };
        }

        public KingdomBuyDecoResPacket KingdomDecoBuy(KingdomBuyDecoReqPacket req)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var mgrKingdomDeco = _userRepo.KingdomDeco.Touch(prtKingdomItem.Num);
            ReqHelper.ValidContext(mgrKingdomDeco.Model.TotalCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_DECO_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = mgrKingdomDeco.Model.TotalCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_DECO:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var chgCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);
            mgrKingdomDeco.Inc(1, reason);
            return new KingdomBuyDecoResPacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mgrKingdomDeco.Model),
                ChgObj = chgCostObj,
            };
        }

        public KingdomConstructStructureResPacket KingdomConstructStructure(KingdomConstructStructureReqPacket req)
        {
            var mgrKingdomStructure = _userRepo.KingdomStructure.Get(req.KingdomStructureId);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var mgrKingdomMap = _userRepo.KingdomMap.Touch();

            // Tile 위치 중복 체크
            var valTileStartPos = mgrKingdomMap.ValidEmptyTile(req.StartTilePos, mgrKingdomStructure.Prt);

            // Cost일치하는지 체크
            var reason = $"CONSTURCT_KINGDOM_STRUCTURE:{req.KingdomStructureId}";
            var prtKingdomItem = mgrKingdomStructure.Prt;
            // TODO: List형태 필요한지 고려해보고 수정
            var valCostObj = ReqHelper.ValidCost(req.CostObjList[0], prtKingdomItem.ConstructObjType, prtKingdomItem.ConstructObjNum, prtKingdomItem.ConstructObjAmount, reason);

            // 처리: 건설 재료 소모
            var chgCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            // DELETEME: Map 형태로 저장 형식 변경            // 처리: 타일 설치
            // var placedKingdomItem = _userRepo.PlacedKingdomItem.Create(mgrKingdomStructure.Prt, reqStartTilePos.X, reqStartTilePos.Y, mgrKingdomStructure);

            // 처리: 건설 시작(상태 변경)
            mgrKingdomMap.ConstructStructure(mgrKingdomStructure, valTileStartPos);
            mgrKingdomStructure.Construct();
            return new KingdomConstructStructureResPacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                PlacedKingdomItemList = mgrKingdomMap.Snapshot.PlacedObjDict.Values.ToList(),
                ChgObjList = new List<ChgObjPacket> { chgCostObj },
            };
        }

        public KingdomConstructDecoResPacket KingdomConstructDeco(KingdomConstructDecoReqPacket req)
        {
            var mgrKingdomDeco = _userRepo.KingdomDeco.Touch(req.KingdomItemNum);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var mgrKingdomMap = _userRepo.KingdomMap.Touch();

            // Tile 위치 중복 체크
            var valTileStartPos = mgrKingdomMap.ValidEmptyTile(req.StartTilePos, mgrKingdomDeco.Prt);

            // DELETEME: Map 형태로 저장 형식 변경 // 처리: 타일 설치
            // var placedKingdomItem = _userRepo.PlacedKingdomItem.Create(mgrKingdomDeco.Prt, reqStartTilePos.X, reqStartTilePos.Y);

            // 처리: 건설 완료 (보유 개수 차감)
            mgrKingdomMap.ConstructDeco(mgrKingdomDeco, valTileStartPos);
            mgrKingdomDeco.Place();

            return new KingdomConstructDecoResPacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mgrKingdomDeco.Model),
                PlacedKingdomItemList = mgrKingdomMap.Snapshot.PlacedObjDict.Values.ToList(),
            };
        }

        public KingdomFinishConstructStructureResPacket KingdomFinishConstructStructure(KingdomFinishConstructStructureReqPacket req)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(req.KingdomStructureId);
            mgrKingdomItem.SetReady(EKingdomItemState.CONSTRUCTING);
            return new KingdomFinishConstructStructureResPacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
            };
        }

        public KingdomChangeItemResPacket KingdomItemChange(KingdomChangeItemReqPacket req)
        {
            var mgrKingdomMap = _userRepo.KingdomMap.Touch();
            
            // Chg + Place 리스트중에 겹치는거 없는지 검증
            var valSnapshot = mgrKingdomMap.ValiePlaceItemsSnapshot(req.StoreKingdomItemIdList, req.ChgKingdomItemList, req.PlaceKingdomItemList, 
                out var valStructureDeltaCntDict, out var valDecoDeltaCntDict);

            // Store + Create 한 변화량으로, 보유 수량 검증
            var mgrKingdomStructureList = _userRepo.KingdomStructure.GetAllList(valStructureDeltaCntDict.Keys.ToList());
            var mgrKingdomDecoList = _userRepo.KingdomDeco.GetAllList(valDecoDeltaCntDict.Keys.ToList());
            foreach (var mgrKingdomStructure in mgrKingdomStructureList)
            {
                var cnt = valStructureDeltaCntDict[mgrKingdomStructure.Model.SfId];
                mgrKingdomStructure.ValidChgAction(cnt);
            }

            foreach (var mgrKingdomDeco in mgrKingdomDecoList)
            {
                var cnt = valDecoDeltaCntDict[mgrKingdomDeco.Model.Num];
                mgrKingdomDeco.ValidChgAction(cnt);
            }

            // 처리
            // Store + Create 한 변화량 적용
            foreach(var mgrKingdomStructure in mgrKingdomStructureList)
            {
                var cnt = valStructureDeltaCntDict[mgrKingdomStructure.Model.SfId];
                if (cnt > 0)
                {
                    mgrKingdomStructure.Store();
                }
                else if (cnt < 0)
                {
                    mgrKingdomStructure.Place();
                }
            }

            foreach (var mgrKingdomDeco in mgrKingdomDecoList)
            {
                var cnt = valDecoDeltaCntDict[mgrKingdomDeco.Model.Num];
                if (cnt > 0)
                {
                    mgrKingdomDeco.Store(cnt);
                }
                else if (cnt < 0)
                {
                    mgrKingdomDeco.Place(-cnt);
                }
            }
            // 맵 스냅샷 저장
            mgrKingdomMap.SaveSnapshot(valSnapshot);

            // 로그

            return new KingdomChangeItemResPacket
            {
                KingdomStructureList = _mapper.Map<List<KingdomStructurePacket>>(mgrKingdomStructureList),
                KingdomDecoList = _mapper.Map<List<KingdomDecoPacket>>(mgrKingdomDecoList),
                PlacedKingdomItemList = mgrKingdomMap.Snapshot.PlacedObjDict.Values.ToList(),
            };
        }

        public KingdomDecTimeStructureResPacket KingdomStructureDecTime(KingdomDecTimeStructureReqPacket req)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(req.KingdomStructureId);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();

            // TODO: 남은 시간, 캐시 보유량 일치하는지 검증
            //

            var cashAmount = mgrPlayerDetail.DecCash(req.CashCost.Amount, $"DEC_TIME_KINGDOM_ITEM:{req.KingdomStructureId}");
            mgrKingdomItem.DecTime();
            return new KingdomDecTimeStructureResPacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
                Cash = mgrPlayerDetail.GetCashPacket(),
            };
        }

        public KingdomFinishCraftStructureResPacket KingdomFinishCraftStructure(KingdomFinishCraftStructureReqPacket req)
        {
            var mgrKingdomItem = _userRepo.KingdomStructure.Get(req.KingdomStructureId);
            mgrKingdomItem.SetReady(EKingdomItemState.CRAFTING);
            return new KingdomFinishCraftStructureResPacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
                ChgObjList = new List<ChgObjPacket>(), // TODO: Creft 결과
            };
        }
        #endregion

        #region GACHA
        public GachaNormalResPacket GachaNormal(GachaNormalReqPacket req)
        {
            var scheduleMgr = _centerRepo.Schedule.Get(req.ScheduleNum, EScheduleTimeType.TOTAL);
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();

            // Cost일치하는지 체크
            var valCnt = scheduleMgr.ValidGachaCnt(req.Cnt);
            var valCost = scheduleMgr.ValidGachaCost(req.CostObj, valCnt);

            // 재화 소모
            var resultCostObj = mgrPlayerDetail.DecCost(valCost, scheduleMgr.MakeGachaReason(valCnt));

            // TODO: 가챠 로직
            
            return new GachaNormalResPacket
            {
               CostChgObj = resultCostObj,
               GachaResultChgObjList = null
            };
        }
        #endregion

        #region COOKIE
        public CookieEnhanceStarResPacket EnhanceCookieStar(CookieEnhanceStarReqPacket req)
        {
           /* var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var hasItemCnt = _userRepo.KingdomStructure.GetKingdomStructureCnt(prtKingdomItem.Num);
            ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_STRUCTURE:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            var mgrKingdomStructure = _userRepo.KingdomStructure.Create(prtKingdomItem);*/
            return new CookieEnhanceStarResPacket
            {
/*                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                ChgObj = resultCostObj,*/
            };
        }

        public CookieEnhanceLvResPacket EnhanceCookieLv(CookieEnhanceLvReqPacket req)
        {
/*            var scheduleMgr = _*/
            /* var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

             // Item 최대 보유량 체크
             var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
             var hasItemCnt = _userRepo.KingdomStructure.GetKingdomStructureCnt(prtKingdomItem.Num);
             ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT",
                 () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

             // Cost일치하는지 체크
             var reason = $"BUY_KINGDOM_STRUCTURE:{req.KingdomItemNum}";
             var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

             var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

             var mgrKingdomStructure = _userRepo.KingdomStructure.Create(prtKingdomItem);*/
            return new CookieEnhanceLvResPacket
            {
                /*                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                                ChgObj = resultCostObj,*/
            };
        }
        #endregion

        private readonly AuthRepo _authRepo;
        private readonly UserRepo _userRepo;
        private readonly CenterRepo _centerRepo;
        private readonly AllUserRepo _allUserRepo;
        private readonly IMapper _mapper;
    }
}

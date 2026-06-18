using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Helper;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class GameService : ServiceBase
    {
        public GameService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<GameService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        #region GAME
        public GameEnterResponsePacket Enter(GameEnterRequestPacket req)
        {
            var mgrPlayer = OwnUser.Player.Touch();

            if (mgrPlayer.Model.State >= Proto.EPlayerState.PREPARED)
            {
                // Prepare 이후 접속시마다 처리해줘야할 것이 있으면 여기서 처리
                var pakPlayer = mgrPlayer.LoadPlayer(_mapper);
                return new GameEnterResponsePacket
                {
                    Player = pakPlayer,
                };
            }
            else
            {
                var pakPlayer = mgrPlayer.PreparePlayer(_mapper);

                var accountId = mgrPlayer.Model.AccountId;
                var authRepo = _dbRepo.Auth;
                authRepo.PlayerMap.Create(new PlayerMapModel
                {
                    AccountId = accountId,
                    PlayerId = mgrPlayer.Id,
                    ShardId = OwnUser.ShardId,
                });

                if (authRepo.Session.TryGetByAccountId(accountId, out var mdlSession))
                {
                    mdlSession.SetPlayerId(mgrPlayer.Id);
                }


                return new GameEnterResponsePacket
                {
                    Player = pakPlayer,
                };
            }
        }

        public GameChangeNameResponsePacket ChangeNameFirst(GameChangeNameRequestPacket req)
        {
            var reqName = req.PlayerName;
            var mgrPlayer = OwnUser.Player.Touch();

            mgrPlayer.ValidState(EPlayerState.CHANGED_NAME_FIRST);

            // 중복 체크 (클라에 팝업)
            ReqHelper.Valid(!_dbRepo.AllUser.TryGetPlayerByName(reqName, out _), EErrorCode.GAME_CHANGE_NAME_EXIST_NAME);

            // 변경
            mgrPlayer.ChangeName(reqName);

            return new GameChangeNameResponsePacket
            {
                PlayerName = mgrPlayer.Model.ProfileName,
            };
        }
        #endregion

        #region KINGDOM
        public KingdomBuyStructureResponsePacket KingdomStructureBuy(KingdomBuyStructureRequestPacket req)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var hasItemCnt = OwnUser.KingdomStructure.GetKingdomStructureCnt(prtKingdomItem.Num);
            ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_STRUCTURE:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);

            var mgrKingdomStructure = OwnUser.KingdomStructure.Create(prtKingdomItem);
            return new KingdomBuyStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                ChgObj = resultCostObj,
            };
        }

        public KingdomBuyDecoResponsePacket KingdomDecoBuy(KingdomBuyDecoRequestPacket req)
        {
            var prtKingdomItem = APP.Prt.GetKingdomItemPrt(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var mgrKingdomDeco = OwnUser.KingdomDeco.Touch(prtKingdomItem.Num);
            ReqHelper.ValidContext(mgrKingdomDeco.Model.TotalCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_DECO_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = mgrKingdomDeco.Model.TotalCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_DECO:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);

            var chgCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);
            mgrKingdomDeco.Inc(1, reason);
            return new KingdomBuyDecoResponsePacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mgrKingdomDeco.Model),
                ChgObj = chgCostObj,
            };
        }

        public KingdomConstructStructureResponsePacket KingdomConstructStructure(KingdomConstructStructureRequestPacket req)
        {
            var mgrKingdomStructure = OwnUser.KingdomStructure.Get(req.KingdomStructureId);
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var mgrKingdomMap = OwnUser.KingdomMap.Touch();

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
            // var placedKingdomItem = OwnUser.PlacedKingdomItem.Create(mgrKingdomStructure.Prt, reqStartTilePos.X, reqStartTilePos.Y, mgrKingdomStructure);

            // 처리: 건설 시작(상태 변경)
            mgrKingdomMap.ConstructStructure(mgrKingdomStructure, valTileStartPos);
            mgrKingdomStructure.Construct();
            return new KingdomConstructStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomStructure.Model),
                PlacedKingdomItemList = [.. mgrKingdomMap.Snapshot.PlacedObjDict.Values],
                ChgObjList = [chgCostObj],
            };
        }

        public KingdomConstructDecoResponsePacket KingdomConstructDeco(KingdomConstructDecoRequestPacket req)
        {
            var mgrKingdomDeco = OwnUser.KingdomDeco.Touch(req.KingdomItemNum);

            _ = OwnUser.PlayerDetail.Touch();
            var mgrKingdomMap = OwnUser.KingdomMap.Touch();

            // Tile 위치 중복 체크
            var valTileStartPos = mgrKingdomMap.ValidEmptyTile(req.StartTilePos, mgrKingdomDeco.Prt);

            // DELETEME: Map 형태로 저장 형식 변경 // 처리: 타일 설치
            // var placedKingdomItem = OwnUser.PlacedKingdomItem.Create(mgrKingdomDeco.Prt, reqStartTilePos.X, reqStartTilePos.Y);

            // 처리: 건설 완료 (보유 개수 차감)
            mgrKingdomMap.ConstructDeco(mgrKingdomDeco, valTileStartPos);
            mgrKingdomDeco.Place();

            return new KingdomConstructDecoResponsePacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mgrKingdomDeco.Model),
                PlacedKingdomItemList = [.. mgrKingdomMap.Snapshot.PlacedObjDict.Values],
            };
        }

        public KingdomFinishConstructStructureResponsePacket KingdomFinishConstructStructure(KingdomFinishConstructStructureRequestPacket req)
        {
            var mgrKingdomItem = OwnUser.KingdomStructure.Get(req.KingdomStructureId);
            mgrKingdomItem.SetReady(EKingdomItemState.CONSTRUCTING);
            return new KingdomFinishConstructStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
            };
        }

        public KingdomChangeItemResponsePacket KingdomItemChange(KingdomChangeItemRequestPacket req)
        {
            var mgrKingdomMap = OwnUser.KingdomMap.Touch();

            // Chg + Place 리스트중에 겹치는거 없는지 검증
            var valSnapshot = mgrKingdomMap.ValiePlaceItemsSnapshot(req.StoreKingdomItemIdList, req.ChgKingdomItemList, req.PlaceKingdomItemList,
                out var valStructureDeltaCntDict, out var valDecoDeltaCntDict);

            // Store + Create 한 변화량으로, 보유 수량 검증
            var mgrKingdomStructureList = OwnUser.KingdomStructure.GetAllList([.. valStructureDeltaCntDict.Keys]);
            var mgrKingdomDecoList = OwnUser.KingdomDeco.GetAllList([.. valDecoDeltaCntDict.Keys]);
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
            foreach (var mgrKingdomStructure in mgrKingdomStructureList)
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

            return new KingdomChangeItemResponsePacket
            {
                KingdomStructureList = _mapper.Map<List<KingdomStructurePacket>>(mgrKingdomStructureList),
                KingdomDecoList = _mapper.Map<List<KingdomDecoPacket>>(mgrKingdomDecoList),
                PlacedKingdomItemList = [.. mgrKingdomMap.Snapshot.PlacedObjDict.Values],
            };
        }

        public KingdomDecTimeStructureResponsePacket KingdomStructureDecTime(KingdomDecTimeStructureRequestPacket req)
        {
            var mgrKingdomItem = OwnUser.KingdomStructure.Get(req.KingdomStructureId);
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();


            // TODO: 남은 시간, 캐시 보유량 일치하는지 검증
            //

            _ = mgrPlayerDetail.DecCash(req.CashCost.Amount, $"DEC_TIME_KINGDOM_ITEM:{req.KingdomStructureId}");
            mgrKingdomItem.DecTime();
            return new KingdomDecTimeStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
                Cash = mgrPlayerDetail.GetCashPacket(),
            };
        }

        public KingdomFinishCraftStructureResponsePacket KingdomFinishCraftStructure(KingdomFinishCraftStructureRequestPacket req)
        {
            var mgrKingdomItem = OwnUser.KingdomStructure.Get(req.KingdomStructureId);
            mgrKingdomItem.SetReady(EKingdomItemState.CRAFTING);
            return new KingdomFinishCraftStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mgrKingdomItem.Model),
                ChgObjList = [], // TODO: Creft 결과
            };
        }
        #endregion

        #region GACHA
        public ScheduleLoadResponsePacket LoadSchedule(ScheduleLoadRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var mgrScheduleList = centerRepo.Schedule.GetList();
            return new ScheduleLoadResponsePacket
            {
                ScheduleList = _mapper.Map<List<SchedulePacket>>(mgrScheduleList),
            };
        }

        public GachaNormalResponsePacket GachaNormal(GachaNormalRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var scheduleMgr = centerRepo.Schedule.Get(req.ScheduleNum, EScheduleTimeType.TOTAL);
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();

            // Cost일치하는지 체크
            var valCnt = scheduleMgr.ValidGachaCnt(req.Cnt);
            var valCost = scheduleMgr.ValidGachaCost(req.CostObj, valCnt);

            // 재화 소모
            var resultCostObj = mgrPlayerDetail.DecCost(valCost, scheduleMgr.MakeGachaReason(valCnt));

            var gachaRandom = new GachaRandom(scheduleMgr.GachaPrt, RpcContext.ServerTime);
            var rewardObjValList = new List<ObjValue>();
            var gachaResultList = new List<GachaResultPacket>();
            for (var i = 0; i < valCnt; i++)
            {
                var resultObjValue = gachaRandom.Roll(isNormal: true);
                rewardObjValList.AddOrInc(resultObjValue);

                GachaResultPacket gachaResult;
                switch (resultObjValue.Key.Type)
                {
                    case EObjType.COOKIE:
                        var prtCookie = APP.Prt.GetCookiePrt(resultObjValue.Key.Num);
                        gachaResult = new GachaResultPacket
                        {
                            ResultObjValue = resultObjValue,
                            SoulStoneNum = prtCookie.SoulStoneNum,
                            SoulStoneAmount = prtCookie.InitSoulStone * (int)resultObjValue.Value
                        };
                        break;
                    case EObjType.SOUL_STONE:
                        gachaResult = new GachaResultPacket
                        {
                            ResultObjValue = resultObjValue,
                            SoulStoneNum = resultObjValue.Key.Num,
                            SoulStoneAmount = (int)resultObjValue.Value
                        };
                        break;
                    default:
                        throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NO_HANDLING_GACHA_RESULT", new { ObjType = resultObjValue.Key.Type });
                }
                gachaResultList.Add(gachaResult);
            }

            // TODO: 가챠 전용 Inc로 ㄱㄱ
            var chgObjList = mgrPlayerDetail.IncRewardList(rewardObjValList, scheduleMgr.MakeGachaReason(valCnt));

            return new GachaNormalResponsePacket
            {
                CostChgObj = resultCostObj,
                GachaResultChgObjList = chgObjList,
                GachaResultList = gachaResultList,
            };
        }
        #endregion

        #region COOKIE
        public CookieEnhanceStarResponsePacket EnhanceCookieStar(CookieEnhanceStarRequestPacket req)
        {
            var mgrCookie = OwnUser.Cookie.Touch(req.CookieNum);
            ReqHelper.ValidContext(req.BefStar == mgrCookie.Model.Star, "NOT_EQUAL_COOKIE_STAR", () => new { CookieNum = mgrCookie.Model.Num, req.BefStar, CookieStar = mgrCookie.Model.Star });
            var deltaLv = req.AftStar - req.BefStar;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_STAR");

            var valUsedSoulStone = mgrCookie.GetSoulStoneByEnhanceStar(mgrCookie.Model.Star, req.AftStar);
            ReqHelper.ValidContext(req.UsedSoulStone == valUsedSoulStone, "NOT_EQUAL_USED_SOUL_STONE", () => new { CookieNum = mgrCookie.Model.Num, req.UsedSoulStone, ValUsedSoulStone = valUsedSoulStone });

            mgrCookie.EnhanceStar(req.AftStar, valUsedSoulStone);

            return new CookieEnhanceStarResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(mgrCookie.Model),
            };
        }

        public CookieEnhanceLvResponsePacket EnhanceCookieLv(CookieEnhanceLvRequestPacket req)
        {
            var mgrCookie = OwnUser.Cookie.Touch(req.CookieNum);
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var cfgLvCost = 10;

            var reason = $"ENHANCE_COOKIE_LV:{req.BefLv}~{req.AftLv}";
            var deltaLv = req.AftLv - req.BefLv;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_LV");
            ReqHelper.ValidContext(req.BefLv == mgrCookie.Model.Lv, "NOT_EQUAL_COOKIE_Lv", () => new { CookieNum = mgrCookie.Model.Num, req.BefLv, CookieLv = mgrCookie.Model.Lv });
            var valCostObj = ReqHelper.ValidCost(req.CostObj, EObjType.POINT_COOKIE_LV, 0, deltaLv * cfgLvCost, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);
            mgrCookie.EnhanceLv(req.AftLv);

            return new CookieEnhanceLvResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(mgrCookie.Model),
                ChgObj = resultCostObj,
            };
        }
        #endregion

        #region WORLD
        public WorldFinishStageFirstResponsePacket WorldFinishStageFirst(WorldFinishStageFirstRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);
            var mgrWorldStage = OwnUser.WorldStage.Touch(req.StageNum);
            ReqHelper.ValidContext(mgrWorld.TryGetTopOpenStagePrt(out var prtNextWorldStage), "NOT_FOUND_TOP_OPEN_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, mgrWorld.Model.TopFinishStageNum });
            ReqHelper.ValidContext(prtNextWorldStage.Num == req.StageNum, "NOT_EQUAL_FIRST_FINISH_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, ReqStageNum = req.StageNum, ValStageNum = prtNextWorldStage.Num });
            ReqHelper.ValidContext(mgrWorld.IsFinishPrevWorld(), "NOT_FINISH_PREV_WORLD", () => new { WorldNum = mgrWorld.Prt.Num });

            // 최초 보상
            var prtRewardList = new List<ObjValue>();
            var firstReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[0], mgrWorldStage.Prt.FirstRewardNumList[0], mgrWorldStage.Prt.FirstRewardAmountList[0]);
            prtRewardList.AddOrInc(firstReward);

            // Star 보상
            ReqHelper.ValidProto(req.Star <= mgrWorldStage.Prt.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = 1; star <= valStar; star++)
            {
                var starReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[star], mgrWorldStage.Prt.FirstRewardNumList[star], mgrWorldStage.Prt.FirstRewardAmountList[star]);
                prtRewardList.AddOrInc(starReward);
            }

            var reason = $"WORLD_FINISH_STAGE_FIRST:{mgrWorldStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(valRewardList, reason);

            mgrWorld.FinishStage(mgrWorldStage.Prt);
            mgrWorldStage.SetStar(valStar);

            return new WorldFinishStageFirstResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = chgObjList,
            };
        }

        public WorldFinishStageRepeatResponsePacket WorldFinishStageRepeat(WorldFinishStageRepeatRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);
            var mgrWorldStage = OwnUser.WorldStage.Touch(req.StageNum);

            ReqHelper.ValidContext(req.StageNum <= mgrWorld.Model.TopFinishStageNum, "NOT_FINISHED_STAGE", () => new { req.WorldNum, req.StageNum, mgrWorld.Model.TopFinishStageNum });

            // Star 보상
            var prtRewardList = new List<ObjValue>();
            ReqHelper.ValidProto(req.Star <= mgrWorldStage.Prt.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = mgrWorldStage.Model.Star + 1; star <= valStar; star++)
            {
                if (star == 0)
                {
                    continue;
                }

                var starReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[star], mgrWorldStage.Prt.FirstRewardNumList[star], mgrWorldStage.Prt.FirstRewardAmountList[star]);
                prtRewardList.AddOrInc(starReward);
            }

            var reason = $"WORLD_FINISH_STAGE_REPEAT:{mgrWorldStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(valRewardList, reason);
            mgrWorld.FinishStage(mgrWorldStage.Prt);
            mgrWorldStage.SetStar(valStar);

            return new WorldFinishStageRepeatResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = chgObjList,
            };
        }

        public WorldRewardStarResponsePacket WorldRewardStar(WorldRewardStarRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);

            var valTotalStar = OwnUser.WorldStage.GetTotalStar(mgrWorld.Model.Num);
            var maxTotalStar = mgrWorld.Prt.RewardStarList[req.AftRewardStar - 1];
            ReqHelper.ValidContext(maxTotalStar <= valTotalStar, "NOT_ENOUGH_TOTAL_STAR", () => new { WorldNum = mgrWorld.Prt.Num, ValTotalStar = valTotalStar, PrtMaxTotalStar = maxTotalStar });
            ReqHelper.ValidContext(req.BefRewardStar >= mgrWorld.Model.RecvStarReward, "ALREADY_RECV_WORLD_STAR_REWARD", () => new { WorldNum = mgrWorld.Prt.Num, ReqBefStar = req.BefRewardStar });

            var prtReward = new ObjValue(EObjType.FREE_CASH, 0, 0);
            for (var starIdx = req.BefRewardStar; starIdx < req.AftRewardStar; starIdx++)
            {
                var cashAmount = mgrWorld.Prt.RewardStarCashList[starIdx];
                prtReward.Value += cashAmount;
            }

            var reason = $"WORLD_REWARD_STAR:{mgrWorld.Prt.Num}:{req.BefRewardStar}~{req.AftRewardStar}";
            var valReward = ReqHelper.ValidReward(req.RewardValue, prtReward, reason);

            // 처리
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            mgrWorld.RewardStar(req.AftRewardStar, valTotalStar);
            var chgObj = mgrPlayerDetail.IncReward(valReward, reason);

            return new WorldRewardStarResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                ChgObj = chgObj
            };
        }
        #endregion

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}

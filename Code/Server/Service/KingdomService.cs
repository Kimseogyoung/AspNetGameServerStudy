using AutoMapper;
using Proto;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Helper;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class KingdomService : ServiceBase
    {
        public KingdomService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<KingdomService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public KingdomBuyStructureResponsePacket KingdomStructureBuy(KingdomBuyStructureRequestPacket req)
        {
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(req.KingdomItemNum);

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
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(req.KingdomItemNum);

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

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}

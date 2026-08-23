using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using ServerCore.Helper;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace Server.Service
{
    public class KingdomService : ServiceBase
    {
        public KingdomService(GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<KingdomService> logger) : base(db, rpcContext, logger)
        {
            _mapper = mapper;
        }

        public async Task<KingdomBuyStructureResponsePacket> KingdomStructureBuyAsync(KingdomBuyStructureRequestPacket req)
        {
            var userScope = OwnScope;
            var structureSet = userScope.Owned<KingdomStructureModel>();
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var hasItemCnt = await structureSet.GetStructureCntAsync(prtKingdomItem.Num);
            ReqHelper.ValidContext(hasItemCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_STRUCTURE_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = hasItemCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_STRUCTURE:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);
            var costChangeList = await RewardService.PayAsync(userScope, valCostObj, reason);

            // SfId 는 여기서 만든다. PK 라 스코프가 정해주지 않는다.
            var mdlStructure = await structureSet.CreateStructureAsync(IdHelper.GenerateSfId(), prtKingdomItem.Num);

            return new KingdomBuyStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mdlStructure),
                ChgObjList = costChangeList.ToPacketList(),
            };
        }

        public async Task<KingdomBuyDecoResponsePacket> KingdomDecoBuyAsync(KingdomBuyDecoRequestPacket req)
        {
            var userScope = OwnScope;
            var decoSet = userScope.Owned<KingdomDecoModel>();
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(req.KingdomItemNum);

            // Item 최대 보유량 체크
            var mdlDeco = await decoSet.GetOrCreateDecoAsync(prtKingdomItem.Num);
            ReqHelper.ValidContext(mdlDeco.TotalCnt < prtKingdomItem.MaxCnt, "FULL_KINGDOM_DECO_CNT",
                () => new { KingdomItemNum = prtKingdomItem.Num, HasItemCnt = mdlDeco.TotalCnt, MaxItemCnt = prtKingdomItem.MaxCnt });

            // Cost일치하는지 체크
            var reason = $"BUY_KINGDOM_DECO:{req.KingdomItemNum}";
            var valCostObj = ReqHelper.ValidCost(req.CostObj, prtKingdomItem.CostObjType, prtKingdomItem.CostObjNum, prtKingdomItem.CostObjAmount, reason);
            var costChangeList = await RewardService.PayAsync(userScope, valCostObj, reason);

            mdlDeco.Inc(1, prtKingdomItem, reason);
            await decoSet.UpdateAsync(mdlDeco);

            return new KingdomBuyDecoResponsePacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mdlDeco),
                ChgObjList = costChangeList.ToPacketList(),
            };
        }

        public async Task<KingdomConstructStructureResponsePacket> KingdomConstructStructureAsync(KingdomConstructStructureRequestPacket req)
        {
            var userScope = OwnScope;
            var structureSet = userScope.Owned<KingdomStructureModel>();

            var mdlStructure = await structureSet.GetStructureAsync(req.KingdomStructureId);
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(mdlStructure.Num);

            // Tile 위치 중복 체크
            var valTileStartPos = await KingdomMapService.ValidEmptyTileAsync(userScope, req.StartTilePos, prtKingdomItem);

            // Cost일치하는지 체크
            var reason = $"CONSTRUCT_KINGDOM_STRUCTURE:{req.KingdomStructureId}";
            var valCostObj = ReqHelper.ValidCost(req.CostObjList[0], prtKingdomItem.ConstructObjType, prtKingdomItem.ConstructObjNum, prtKingdomItem.ConstructObjAmount, reason);

            // 처리: 건설 재료 소모
            var costChangeList = await RewardService.PayAsync(userScope, valCostObj, reason);

            var snapshot = await KingdomMapService.PlaceItemAsync(userScope, prtKingdomItem, valTileStartPos, mdlStructure.SfId);

            mdlStructure.Construct(prtKingdomItem, RpcContext.ServerTime);
            await structureSet.UpdateAsync(mdlStructure);

            return new KingdomConstructStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mdlStructure),
                PlacedKingdomItemList = [.. snapshot.PlacedObjDict.Values],
                ChgObjList = costChangeList.ToPacketList(),
            };
        }

        public async Task<KingdomConstructDecoResponsePacket> KingdomConstructDecoAsync(KingdomConstructDecoRequestPacket req)
        {
            var userScope = OwnScope;
            var decoSet = userScope.Owned<KingdomDecoModel>();

            var mdlDeco = await decoSet.GetOrCreateDecoAsync(req.KingdomItemNum);
            var prtKingdomItem = ProtoDb.Get<KingdomItemProto>(mdlDeco.Num);

            // Tile 위치 중복 체크
            var valTileStartPos = await KingdomMapService.ValidEmptyTileAsync(userScope, req.StartTilePos, prtKingdomItem);

            var snapshot = await KingdomMapService.PlaceItemAsync(userScope, prtKingdomItem, valTileStartPos, 0);

            mdlDeco.Place();
            await decoSet.UpdateAsync(mdlDeco);

            return new KingdomConstructDecoResponsePacket
            {
                KingdomDeco = _mapper.Map<KingdomDecoPacket>(mdlDeco),
                PlacedKingdomItemList = [.. snapshot.PlacedObjDict.Values],
            };
        }

        public async Task<KingdomFinishConstructStructureResponsePacket> KingdomFinishConstructStructureAsync(KingdomFinishConstructStructureRequestPacket req)
        {
            var structureSet = OwnScope.Owned<KingdomStructureModel>();
            var mdlStructure = await structureSet.GetStructureAsync(req.KingdomStructureId);

            mdlStructure.SetReady(EKingdomItemState.CONSTRUCTING, RpcContext.ServerTime);
            await structureSet.UpdateAsync(mdlStructure);

            return new KingdomFinishConstructStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mdlStructure),
            };
        }

        public async Task<KingdomChangeItemResponsePacket> KingdomItemChangeAsync(KingdomChangeItemRequestPacket req)
        {
            var userScope = OwnScope;
            var structureSet = userScope.Owned<KingdomStructureModel>();
            var decoSet = userScope.Owned<KingdomDecoModel>();

            // Chg + Place 리스트중에 겹치는거 없는지 검증
            var (valSnapshot, valStructureDeltaCntDict, valDecoDeltaCntDict) =
                await KingdomMapService.ValidPlaceItemsAsync(userScope, req.StoreKingdomItemIdList, req.ChgKingdomItemList, req.PlaceKingdomItemList);

            // Store + Create 한 변화량으로, 보유 수량 검증
            var mdlStructureList = await structureSet.GetStructureListAsync([.. valStructureDeltaCntDict.Keys]);
            var mdlDecoList = await decoSet.GetDecoListAsync([.. valDecoDeltaCntDict.Keys]);
            foreach (var mdlStructure in mdlStructureList)
            {
                mdlStructure.ValidChgAction(valStructureDeltaCntDict[mdlStructure.SfId]);
            }

            foreach (var mdlDeco in mdlDecoList)
            {
                mdlDeco.ValidChgAction(valDecoDeltaCntDict[mdlDeco.Num]);
            }

            // 처리: Store + Create 한 변화량 적용
            foreach (var mdlStructure in mdlStructureList)
            {
                var cnt = valStructureDeltaCntDict[mdlStructure.SfId];
                if (cnt > 0)
                {
                    mdlStructure.Store();
                }
                else if (cnt < 0)
                {
                    mdlStructure.Place();
                }

                await structureSet.UpdateAsync(mdlStructure);
            }

            foreach (var mdlDeco in mdlDecoList)
            {
                var cnt = valDecoDeltaCntDict[mdlDeco.Num];
                if (cnt > 0)
                {
                    mdlDeco.Store(cnt);
                }
                else if (cnt < 0)
                {
                    mdlDeco.Place(-cnt);
                }

                await decoSet.UpdateAsync(mdlDeco);
            }

            // 맵 스냅샷 저장
            await KingdomMapService.SaveSnapshotAsync(userScope, valSnapshot);

            return new KingdomChangeItemResponsePacket
            {
                KingdomStructureList = _mapper.Map<List<KingdomStructurePacket>>(mdlStructureList),
                KingdomDecoList = _mapper.Map<List<KingdomDecoPacket>>(mdlDecoList),
                PlacedKingdomItemList = [.. valSnapshot.PlacedObjDict.Values],
            };
        }

        public async Task<KingdomDecTimeStructureResponsePacket> KingdomStructureDecTimeAsync(KingdomDecTimeStructureRequestPacket req)
        {
            var userScope = OwnScope;
            var structureSet = userScope.Owned<KingdomStructureModel>();
            var mdlStructure = await structureSet.GetStructureAsync(req.KingdomStructureId);

            // TODO: 남은 시간, 캐시 보유량 일치하는지 검증
            //

            _ = await RewardService.DecCashAsync(userScope, req.CashCost.Amount, $"DEC_TIME_KINGDOM_ITEM:{req.KingdomStructureId}");

            mdlStructure.DecTime();
            await structureSet.UpdateAsync(mdlStructure);

            // 차감 뒤의 값을 실어야 하므로 차감 후에 읽는다.
            var detail = await userScope.Owned<PlayerDetailModel>().GetOrCreateAsync();
            return new KingdomDecTimeStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mdlStructure),
                Cash = new CashPacket { FreeCash = detail.FreeCash, RealCash = detail.RealCash },
            };
        }

        public async Task<KingdomFinishCraftStructureResponsePacket> KingdomFinishCraftStructureAsync(KingdomFinishCraftStructureRequestPacket req)
        {
            var structureSet = OwnScope.Owned<KingdomStructureModel>();
            var mdlStructure = await structureSet.GetStructureAsync(req.KingdomStructureId);

            mdlStructure.SetReady(EKingdomItemState.CRAFTING, RpcContext.ServerTime);
            await structureSet.UpdateAsync(mdlStructure);

            return new KingdomFinishCraftStructureResponsePacket
            {
                KingdomStructure = _mapper.Map<KingdomStructurePacket>(mdlStructure),
                ChgObjList = [], // TODO: Creft 결과
            };
        }

        private readonly IMapper _mapper;
    }
}

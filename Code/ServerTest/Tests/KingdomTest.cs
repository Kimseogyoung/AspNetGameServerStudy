using Proto;
using Protocol;
using ServerCore;
using WebStudyServer.Model;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// Kingdom 관련 API 테스트
    /// Proto 데이터 기준:
    ///   KingdomItem 110003: STRUCTURE, GOLD=500, MaxCnt=3, ConstructCost ITEM:19001=5, ConstructTime=10s
    ///   KingdomItem 130001: DECO, GOLD=2000, MaxCnt=10000
    /// </summary>
    public class KingdomTest : TestBase
    {
        private const int StructureItemNum = 110003;     // 쿠키 하우스(테스트용), Gold=500
        private const double StructureBuyCost = 500;
        private const double StructureConstructCost = 5; // ITEM:19001
        private const int StructureConstructItemNum = 19001;

        private const int DecoItemNum = 130001;          // 설탕 노움의 집, Gold=2000
        private const double DecoBuyCost = 2000;

        public KingdomTest(GameServerFactory factory) : base(factory) { }

        private async Task GiveGoldAsync(double amount)
        {
            var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                new CheatRewardRequestPacket(new List<ObjValue>
                {
                    new ObjValue(EObjType.GOLD, 0, amount)
                }));
            Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
        }

        private async Task GiveConstructItemAsync(double amount)
        {
            var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                new CheatRewardRequestPacket(new List<ObjValue>
                {
                    new ObjValue(EObjType.ITEM, StructureConstructItemNum, amount)
                }));
            Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
        }

        [Fact]
        public async Task KingdomBuyStructure_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);

            // [성공] 구매
            {
                var res = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                    new KingdomBuyStructureRequestPacket(
                        StructureItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.KingdomStructure);
                Assert.Equal(StructureItemNum, res.KingdomStructure.Num);
            }

            // [실패] 잘못된 비용 Amount
            {
                var res = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                    new KingdomBuyStructureRequestPacket(
                        StructureItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = 1 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 잘못된 비용 타입
            {
                var res = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                    new KingdomBuyStructureRequestPacket(
                        StructureItemNum,
                        new CostObjPacket { Type = EObjType.ITEM, Num = 0, Amount = StructureBuyCost }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 존재하지 않는 KingdomItemNum
            {
                var res = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                    new KingdomBuyStructureRequestPacket(
                        999999,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task KingdomConstructStructure_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);
            await GiveConstructItemAsync(100);

            // 구조물 구매
            // 생성자: (ulong id, int kingdomItemNum, List<CostObj> costs, TilePos startPos)
            var buyRes = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                new KingdomBuyStructureRequestPacket(
                    StructureItemNum,
                    new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                ));
            Assert.Equal((int)EErrorCode.OK, buyRes.Info.ResultCode);
            var structureId = buyRes.KingdomStructure.SfId;

            // [성공] 빈 타일에 건설
            {
                var res = await Api.PostAsync<KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(
                    new KingdomConstructStructureRequestPacket(
                        structureId,
                        StructureItemNum,
                        new List<CostObjPacket>
                        {
                            new CostObjPacket { Type = EObjType.ITEM, Num = StructureConstructItemNum, Amount = StructureConstructCost }
                        },
                        new TilePosPacket { X = 0, Y = 0 }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.KingdomStructure);
            }

            // [실패] 이미 점유된 타일에 건설
            {
                var buyRes2 = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                    new KingdomBuyStructureRequestPacket(
                        StructureItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                    ));
                Assert.Equal((int)EErrorCode.OK, buyRes2.Info.ResultCode);

                var res = await Api.PostAsync<KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(
                    new KingdomConstructStructureRequestPacket(
                        buyRes2.KingdomStructure.SfId,
                        StructureItemNum,
                        new List<CostObjPacket>
                        {
                            new CostObjPacket { Type = EObjType.ITEM, Num = StructureConstructItemNum, Amount = StructureConstructCost }
                        },
                        new TilePosPacket { X = 0, Y = 0 } // 이미 점유된 타일
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 존재하지 않는 구조물 ID
            {
                var res = await Api.PostAsync<KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(
                    new KingdomConstructStructureRequestPacket(
                        9999999ul,
                        StructureItemNum,
                        new List<CostObjPacket>
                        {
                            new CostObjPacket { Type = EObjType.ITEM, Num = StructureConstructItemNum, Amount = StructureConstructCost }
                        },
                        new TilePosPacket { X = 5, Y = 5 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task KingdomFinishConstructStructure_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);
            await GiveConstructItemAsync(100);

            // 구매 → 건설
            var buyRes = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                new KingdomBuyStructureRequestPacket(
                    StructureItemNum,
                    new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                ));
            Assert.Equal((int)EErrorCode.OK, buyRes.Info.ResultCode);
            var structureId = buyRes.KingdomStructure.SfId;

            await Api.PostAsync<KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(
                new KingdomConstructStructureRequestPacket(
                    structureId,
                    StructureItemNum,
                    new List<CostObjPacket>
                    {
                        new CostObjPacket { Type = EObjType.ITEM, Num = StructureConstructItemNum, Amount = StructureConstructCost }
                    },
                    new TilePosPacket { X = 0, Y = 0 }
                ));

            // [실패] 건설 시간(10초)이 남았는데 완료 요청.
            // 이 단언은 예전에 Assert.Equal(OK) 였다. SetReady 의 부등호가 반대여서
            // 즉시 완료가 통과했고, 테스트가 그 동작을 굳혀두고 있었다.
            {
                var res = await Api.PostAsync<KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(
                    new KingdomFinishConstructStructureRequestPacket(structureId, StructureItemNum));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 없는 구조물 ID
            {
                var res = await Api.PostAsync<KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(
                    new KingdomFinishConstructStructureRequestPacket(9999999ul, StructureItemNum));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        // 완료 성공까지는 API 로 못 간다 - 건설이 끝나기를 10초 기다려야 한다.
        // 모델을 직접 불러 시간/상태 경계만 확인한다.
        [Fact]
        public void KingdomStructureSetReady_Test()
        {
            var now = DateTime.UtcNow;

            // [성공] 건설 시간이 지났다
            {
                var mdl = new KingdomStructureModel { State = EKingdomItemState.CONSTRUCTING, EndTime = now.AddSeconds(-1) };
                mdl.SetReady(EKingdomItemState.CONSTRUCTING, now);

                Assert.Equal(EKingdomItemState.READY, mdl.State);
                Assert.Equal(DateTime.MinValue, mdl.EndTime);
            }

            // [실패] 아직 건설 중이다
            {
                var mdl = new KingdomStructureModel { State = EKingdomItemState.CONSTRUCTING, EndTime = now.AddSeconds(10) };
                var ex = Assert.Throws<GameException>(() => mdl.SetReady(EKingdomItemState.CONSTRUCTING, now));

                Assert.Equal("NOT_FINISHED_KINGDOM_STRUCTURE", ex.Message);
            }

            // [실패] 상태가 다르다
            {
                var mdl = new KingdomStructureModel { State = EKingdomItemState.READY, EndTime = DateTime.MinValue };
                var ex = Assert.Throws<GameException>(() => mdl.SetReady(EKingdomItemState.CONSTRUCTING, now));

                Assert.Equal("NOT_EQUAL_CORRECT_BEF_KINGDOM_STRUCTURE_STATE", ex.Message);
            }
        }

        [Fact]
        public async Task KingdomBuyDeco_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);

            // [성공] 데코 구매
            {
                var res = await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                    new KingdomBuyDecoRequestPacket(
                        DecoItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = DecoBuyCost }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.KingdomDeco);
                Assert.Equal(DecoItemNum, res.KingdomDeco.Num);
            }

            // [실패] 잘못된 비용
            {
                var res = await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                    new KingdomBuyDecoRequestPacket(
                        DecoItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = 1 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task KingdomConstructDeco_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);

            // 데코 구매
            var buyRes = await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                new KingdomBuyDecoRequestPacket(
                    DecoItemNum,
                    new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = DecoBuyCost }
                ));
            Assert.Equal((int)EErrorCode.OK, buyRes.Info.ResultCode);

            // [성공] 빈 타일에 설치
            // 생성자: (int kingdomItemNum, TilePosPacket startPos)
            {
                var res = await Api.PostAsync<KingdomConstructDecoRequestPacket, KingdomConstructDecoResponsePacket>(
                    new KingdomConstructDecoRequestPacket(DecoItemNum, new TilePosPacket { X = 0, Y = 0 }));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.KingdomDeco);
            }

            // [실패] 같은 타일 중복 설치
            {
                await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                    new KingdomBuyDecoRequestPacket(
                        DecoItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = DecoBuyCost }
                    ));

                var res = await Api.PostAsync<KingdomConstructDecoRequestPacket, KingdomConstructDecoResponsePacket>(
                    new KingdomConstructDecoRequestPacket(DecoItemNum, new TilePosPacket { X = 0, Y = 0 }));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task KingdomChangeItemStoreDeco_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);

            await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                new KingdomBuyDecoRequestPacket(
                    DecoItemNum,
                    new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = DecoBuyCost }
                ));

            var placeRes = await Api.PostAsync<KingdomConstructDecoRequestPacket, KingdomConstructDecoResponsePacket>(
                new KingdomConstructDecoRequestPacket(DecoItemNum, new TilePosPacket { X = 10, Y = 10 }));
            Assert.Equal((int)EErrorCode.OK, placeRes.Info.ResultCode);

            var placedItem = placeRes.PlacedKingdomItemList.Find(x => x.StartTileX == 10 && x.StartTileY == 10);
            Assert.NotNull(placedItem);
            Assert.Equal(EKingdomItemType.DECO, placedItem.Type);

            // [성공] 방금 놓은 데코를 보관한다.
            // 단건 배치가 타입을 STRUCTURE 로 박아 넣던 시절에는 보관이 구조물 조회로 새어
            // NOT_EQUAL_KINGDOM_ITEM_LIST 로 막혔다.
            {
                var res = await Api.PostAsync<KingdomChangeItemRequestPacket, KingdomChangeItemResponsePacket>(
                    new KingdomChangeItemRequestPacket([placedItem.Id], [], []));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.DoesNotContain(res.PlacedKingdomItemList, x => x.Id == placedItem.Id);
            }
        }

        [Fact]
        public async Task KingdomChangeItemOverlap_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);

            for (var i = 0; i < 2; i++)
            {
                var buyRes = await Api.PostAsync<KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(
                    new KingdomBuyDecoRequestPacket(
                        DecoItemNum,
                        new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = DecoBuyCost }
                    ));
                Assert.Equal((int)EErrorCode.OK, buyRes.Info.ResultCode);
            }

            // 하나를 (10,10) 에 놓는다. 2x2 라 (10,10)~(11,11) 을 차지한다.
            {
                var res = await Api.PostAsync<KingdomChangeItemRequestPacket, KingdomChangeItemResponsePacket>(
                    new KingdomChangeItemRequestPacket([], [], [MakePlaceReq(10, 10)]));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 다음 요청에서 (11,11) 에 겹쳐 놓기.
            // 배치가 시작 칸 하나만 마킹해 저장되던 시절에는 나머지 칸이 빈 칸으로 남아 통과했다.
            {
                var res = await Api.PostAsync<KingdomChangeItemRequestPacket, KingdomChangeItemResponsePacket>(
                    new KingdomChangeItemRequestPacket([], [], [MakePlaceReq(11, 11)]));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        private static ChgKingdomItemPacket MakePlaceReq(int x, int y)
        {
            return new ChgKingdomItemPacket
            {
                PlacedItemId = 0,
                StructureId = 0,
                Num = DecoItemNum,
                TilePos = new TilePosPacket { X = x, Y = y },
            };
        }

        [Fact]
        public async Task KingdomFinishCraftStructure_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);
            await GiveConstructItemAsync(100);

            // 구매 → 건설 (state: CONSTRUCTING)
            // NOTE: StartCraft API 가 없어 CRAFTING 상태로는 못 간다.
            //       CRAFTING 이 아닌 상태에서 거절되는지만 본다.
            var buyRes = await Api.PostAsync<KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(
                new KingdomBuyStructureRequestPacket(
                    StructureItemNum,
                    new CostObjPacket { Type = EObjType.GOLD, Num = 0, Amount = StructureBuyCost }
                ));
            Assert.Equal((int)EErrorCode.OK, buyRes.Info.ResultCode);
            var structureId = buyRes.KingdomStructure.SfId;

            await Api.PostAsync<KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(
                new KingdomConstructStructureRequestPacket(
                    structureId,
                    StructureItemNum,
                    new List<CostObjPacket>
                    {
                        new CostObjPacket { Type = EObjType.ITEM, Num = StructureConstructItemNum, Amount = StructureConstructCost }
                    },
                    new TilePosPacket { X = 0, Y = 0 }
                ));

            // [실패] CONSTRUCTING 상태 구조물에 FinishCraft 요청 (CRAFTING 상태 필요)
            {
                var res = await Api.PostAsync<KingdomFinishCraftStructureRequestPacket, KingdomFinishCraftStructureResponsePacket>(
                    new KingdomFinishCraftStructureRequestPacket(structureId, StructureItemNum));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 존재하지 않는 구조물 ID
            {
                var res = await Api.PostAsync<KingdomFinishCraftStructureRequestPacket, KingdomFinishCraftStructureResponsePacket>(
                    new KingdomFinishCraftStructureRequestPacket(9999999ul, StructureItemNum));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

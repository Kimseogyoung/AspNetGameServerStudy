using Proto;
using Protocol;
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

            // [성공] 건설 완료
            // 생성자: (ulong id, int kingdomItemNum)
            {
                var res = await Api.PostAsync<KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(
                    new KingdomFinishConstructStructureRequestPacket(structureId, StructureItemNum));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.KingdomStructure);
            }

            // [실패] 이미 완료된 구조물에 다시 완료 요청 (상태 불일치)
            {
                var res = await Api.PostAsync<KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(
                    new KingdomFinishConstructStructureRequestPacket(structureId, StructureItemNum));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
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
        public async Task KingdomFinishCraftStructure_Test()
        {
            await CreateDummyPlayerAsync();
            await GiveGoldAsync(100000);
            await GiveConstructItemAsync(100);

            // 구매 → 건설 → 건설완료 (state: READY)
            // NOTE: StartCraft API가 없으므로 CRAFTING 상태 진입 불가.
            //       현재는 READY 상태에서 호출하는 실패 케이스만 검증 가능.
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

            var finishConstructRes = await Api.PostAsync<KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(
                new KingdomFinishConstructStructureRequestPacket(structureId, StructureItemNum));
            Assert.Equal((int)EErrorCode.OK, finishConstructRes.Info.ResultCode);

            // [실패] READY 상태 구조물에 FinishCraft 요청 (CRAFTING 상태 필요)
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

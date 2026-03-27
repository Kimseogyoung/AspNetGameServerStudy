using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// Cookie 관련 API 테스트
    /// Proto 데이터 기준:
    ///   Cookie 1010: COMMON 등급, SoulStoneNum=1011, InitSoulStone=20
    ///   CookieStarEnhance COMMON, Star=0 → 필요 소울스톤 20개
    ///   CookieEnhanceLv: cfgLvCost=10 per lv, CostObjType=POINT_COOKIE_LV
    /// </summary>
    public class CookieTest : TestBase
    {
        private const int CookieNum = 1010;          // DefaultPlayer에 포함된 쿠키
        private const int SoulStoneNum = 1011;        // Cookie 1010의 소울스톤 번호
        private const int SoulStoneForStar0To1 = 20;  // Star 0→1 필요 소울스톤

        public CookieTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task CookieEnhanceStar_Test()
        {
            await CreateDummyPlayerAsync();

            // 소울스톤 지급
            var cheatRes = await Api.PostAsync<CheatRewardReqPacket, CheatRewardResPacket>(
                new CheatRewardReqPacket(new List<ObjValue>
                {
                    new ObjValue(EObjType.SOUL_STONE, SoulStoneNum, 1000)
                }));
            Assert.Equal((int)EErrorCode.OK, cheatRes.Info.ResultCode);

            // [성공] Star 0 → 1 (소울스톤 20개 소모)
            {
                var res = await Api.PostAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(
                    new CookieEnhanceStarReqPacket(
                        cookienum: CookieNum,
                        befstar: 0,
                        aftstar: 1,
                        usedsoulstone: SoulStoneForStar0To1
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.Cookie);
                Assert.Equal(1, res.Cookie.Star);
            }

            // [실패] BefStar 불일치 (현재 Star=1인데 BefStar=0으로 요청)
            {
                var res = await Api.PostAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(
                    new CookieEnhanceStarReqPacket(
                        cookienum: CookieNum,
                        befstar: 0,  // 실제는 1
                        aftstar: 1,
                        usedsoulstone: SoulStoneForStar0To1
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] UsedSoulStone 불일치
            {
                var res = await Api.PostAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(
                    new CookieEnhanceStarReqPacket(
                        cookienum: CookieNum,
                        befstar: 1,
                        aftstar: 2,
                        usedsoulstone: 1  // 실제 필요량과 다름
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] AftStar < BefStar (감소 요청)
            {
                var res = await Api.PostAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(
                    new CookieEnhanceStarReqPacket(
                        cookienum: CookieNum,
                        befstar: 1,
                        aftstar: 0,  // 감소
                        usedsoulstone: 0
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 존재하지 않는 쿠키 번호
            {
                var res = await Api.PostAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(
                    new CookieEnhanceStarReqPacket(
                        cookienum: 99999,
                        befstar: 0,
                        aftstar: 1,
                        usedsoulstone: SoulStoneForStar0To1
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task CookieEnhanceLv_Test()
        {
            await CreateDummyPlayerAsync();

            // Lv 포인트 지급 (cfgLvCost=10 per lv)
            var cheatRes = await Api.PostAsync<CheatRewardReqPacket, CheatRewardResPacket>(
                new CheatRewardReqPacket(new List<ObjValue>
                {
                    new ObjValue(EObjType.POINT_COOKIE_LV, 0, 100000)
                }));
            Assert.Equal((int)EErrorCode.OK, cheatRes.Info.ResultCode);

            // [성공] Lv 1 → 2 (비용 10 POINT_COOKIE_LV)
            {
                var res = await Api.PostAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(
                    new CookieEnhanceLvReqPacket(
                        cookienum: CookieNum,
                        beflv: 1,
                        aftlv: 2,
                        costobj: new CostObjPacket { Type = EObjType.POINT_COOKIE_LV, Num = 0, Amount = 10 }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.Cookie);
                Assert.Equal(2, res.Cookie.Lv);
            }

            // [성공] Lv 2 → 5 (비용 30 POINT_COOKIE_LV)
            {
                var res = await Api.PostAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(
                    new CookieEnhanceLvReqPacket(
                        cookienum: CookieNum,
                        beflv: 2,
                        aftlv: 5,
                        costobj: new CostObjPacket { Type = EObjType.POINT_COOKIE_LV, Num = 0, Amount = 30 }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.Equal(5, res.Cookie.Lv);
            }

            // [실패] BefLv 불일치 (현재 Lv=5인데 BefLv=1로 요청)
            {
                var res = await Api.PostAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(
                    new CookieEnhanceLvReqPacket(
                        cookienum: CookieNum,
                        beflv: 1,
                        aftlv: 2,
                        costobj: new CostObjPacket { Type = EObjType.POINT_COOKIE_LV, Num = 0, Amount = 10 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 비용 Amount 불일치
            {
                var res = await Api.PostAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(
                    new CookieEnhanceLvReqPacket(
                        cookienum: CookieNum,
                        beflv: 5,
                        aftlv: 6,
                        costobj: new CostObjPacket { Type = EObjType.POINT_COOKIE_LV, Num = 0, Amount = 1 } // 틀린 금액
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] AftLv < BefLv (감소 요청)
            {
                var res = await Api.PostAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(
                    new CookieEnhanceLvReqPacket(
                        cookienum: CookieNum,
                        beflv: 5,
                        aftlv: 3,
                        costobj: new CostObjPacket { Type = EObjType.POINT_COOKIE_LV, Num = 0, Amount = 0 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

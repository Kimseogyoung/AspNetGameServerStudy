using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// cheat/reward 테스트
    /// - 다른 테스트에서 리소스 지급용으로 사용하는 API
    /// </summary>
    public class CheatTest : TestBase
    {
        public CheatTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task CheatReward_Test()
        {
            await CreateDummyPlayerAsync();

            // [성공] Gold 지급
            {
                var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                    new CheatRewardRequestPacket(new List<ObjValue>
                    {
                        new ObjValue(EObjType.GOLD, 0, 10000)
                    }));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.ChgObjList);
                Assert.True(res.ChgObjList.Count > 0);
            }

            // [성공] 여러 재화 동시 지급
            {
                var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                    new CheatRewardRequestPacket(new List<ObjValue>
                    {
                        new ObjValue(EObjType.GOLD, 0, 5000),
                        new ObjValue(EObjType.FREE_CASH, 0, 1000),  // TOTAL_CASH는 감소 전용 복합타입 — FREE_CASH로 지급
                        new ObjValue(EObjType.SOUL_STONE, 1011, 100),
                        new ObjValue(EObjType.POINT_COOKIE_LV, 0, 500),
                    }));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.True(res.ChgObjList.Count >= 4);
            }

            // [성공] 빈 목록 지급 (0개 보상도 허용)
            {
                var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                    new CheatRewardRequestPacket(new List<ObjValue>()));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 세션 없이 요청
            {
                var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                    new CheatRewardRequestPacket(new List<ObjValue>
                    {
                        new ObjValue(EObjType.GOLD, 0, 1000)
                    }), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 유효하지 않은 세션키
            {
                var res = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                    new CheatRewardRequestPacket(new List<ObjValue>
                    {
                        new ObjValue(EObjType.GOLD, 0, 1000)
                    }), sessionKey: "INVALID_SESSION");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

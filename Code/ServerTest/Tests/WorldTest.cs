using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// World 관련 API 테스트
    /// Proto 데이터 기준:
    ///   World 110100: NORMAL, Order=1
    ///   WorldStage 11010010: WorldNum=110100, Order=10 (첫번째 스테이지)
    ///     FirstReward[0]: FREE_CASH=20 (기본)
    ///     StarReward[1]: NONE=0 (star1)
    ///     StarReward[2]: FREE_CASH=50 (star2)
    ///     StarReward[3]: FREE_CASH=100 (star3)
    ///   World RewardStar: [10star=300cash, 20star=500cash, 30star=700cash]
    ///
    /// WorldRewardStarReqPacket 생성자: (worldnum, befRewardStar, aftRewardStar, totalStar, rewardValue)
    /// </summary>
    public class WorldTest : TestBase
    {
        private const int WorldNum = 110100;
        private const int FirstStageNum = 11010010;  // World 110100의 첫 스테이지 (Order=10)

        public WorldTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task WorldFinishStageFirst_Test()
        {
            await CreateDummyPlayerAsync();

            // [성공] 첫 스테이지 클리어 (0스타, 기본보상 FREE_CASH=20)
            {
                var res = await Api.PostAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(
                    new WorldFinishStageFirstReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>
                        {
                            new ObjValue(EObjType.FREE_CASH, 0, 20)
                        }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.World);
                Assert.NotNull(res.WorldStage);
            }

            // [실패] 이미 클리어한 스테이지를 다시 FirstFinish 요청
            {
                var res = await Api.PostAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(
                    new WorldFinishStageFirstReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>
                        {
                            new ObjValue(EObjType.FREE_CASH, 0, 20)
                        }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 잘못된 보상 금액
            {
                await CreateDummyPlayerAsync(); // 새 플레이어

                var res = await Api.PostAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(
                    new WorldFinishStageFirstReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>
                        {
                            new ObjValue(EObjType.FREE_CASH, 0, 9999) // 틀린 금액
                        }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 세션 없이 요청
            {
                var res = await Api.PostAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(
                    new WorldFinishStageFirstReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>
                        {
                            new ObjValue(EObjType.FREE_CASH, 0, 20)
                        }
                    ), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task WorldFinishStageRepeat_Test()
        {
            await CreateDummyPlayerAsync();

            // 먼저 첫 클리어
            var firstRes = await Api.PostAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(
                new WorldFinishStageFirstReqPacket(
                    worldnum: WorldNum,
                    stagenum: FirstStageNum,
                    star: 0,
                    rewardvaluelist: new List<ObjValue>
                    {
                        new ObjValue(EObjType.FREE_CASH, 0, 20)
                    }
                ));
            Assert.Equal((int)EErrorCode.OK, firstRes.Info.ResultCode);

            // [성공] 반복 클리어 (0스타, 추가 보상 없음)
            {
                var res = await Api.PostAsync<WorldFinishStageRepeatReqPacket, WorldFinishStageRepeatResPacket>(
                    new WorldFinishStageRepeatReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>()
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.World);
            }

            // [성공] 반복 클리어에서 스타 업그레이드 (0→2스타, star2 추가 보상 FREE_CASH=50)
            {
                var res = await Api.PostAsync<WorldFinishStageRepeatReqPacket, WorldFinishStageRepeatResPacket>(
                    new WorldFinishStageRepeatReqPacket(
                        worldnum: WorldNum,
                        stagenum: FirstStageNum,
                        star: 2,
                        rewardvaluelist: new List<ObjValue>
                        {
                            new ObjValue(EObjType.FREE_CASH, 0, 50)
                        }
                    ));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.Equal(2, res.WorldStage.Star);
            }

            // [실패] 미클리어 스테이지에 Repeat 요청
            {
                var nextStageNum = 11010020;
                var res = await Api.PostAsync<WorldFinishStageRepeatReqPacket, WorldFinishStageRepeatResPacket>(
                    new WorldFinishStageRepeatReqPacket(
                        worldnum: WorldNum,
                        stagenum: nextStageNum,
                        star: 0,
                        rewardvaluelist: new List<ObjValue>()
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task WorldRewardStar_Test()
        {
            await CreateDummyPlayerAsync();

            // [실패] 스타가 부족한 상태에서 별 보상 요청
            // WorldRewardStarReqPacket 생성자: (worldnum, befRewardStar, aftRewardStar, totalStar, rewardValue)
            {
                var res = await Api.PostAsync<WorldRewardStarReqPacket, WorldRewardStarResPacket>(
                    new WorldRewardStarReqPacket(
                        worldnum: WorldNum,
                        befrewardstar: 0,
                        aftrewardstar: 1,
                        totalstar: 0,  // 실제 획득 스타 0 → 조건 미충족
                        rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 300)
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 잘못된 보상값 (FREE_CASH 금액 틀림)
            {
                var res = await Api.PostAsync<WorldRewardStarReqPacket, WorldRewardStarResPacket>(
                    new WorldRewardStarReqPacket(
                        worldnum: WorldNum,
                        befrewardstar: 0,
                        aftrewardstar: 1,
                        totalstar: 0,
                        rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 1) // 틀린 금액
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 세션 없이 요청
            {
                var res = await Api.PostAsync<WorldRewardStarReqPacket, WorldRewardStarResPacket>(
                    new WorldRewardStarReqPacket(
                        worldnum: WorldNum,
                        befrewardstar: 0,
                        aftrewardstar: 1,
                        totalstar: 0,
                        rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 300)
                    ), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

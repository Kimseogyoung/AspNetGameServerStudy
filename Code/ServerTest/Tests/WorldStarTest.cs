using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// 월드 별 보상 경로. 집계가 없는 컬럼을 보고 WorldNum 도 안 채워져 있어서
    /// 이 API 는 한 번도 성공한 적이 없었다(S9 에서 수정).
    /// </summary>
    public class WorldStarTest : TestBase
    {
        private const int WorldNum = 110100;

        // Order 10/20/30/40. 3별로 깨면 스테이지당 3개 -> 4개면 12개.
        private static readonly int[] StageNumList = [11010010, 11010020, 11010030, 11010040];

        // 스테이지 별 보상: 0성 FREE_CASH 20 / 1성 NONE / 2성 50 / 3성 100
        // AddOrInc 가 NONE 을 버리고 같은 키를 합치므로 3별이면 170 하나다.
        private const int ThreeStarReward = 170;

        public WorldStarTest(GameServerFactory factory) : base(factory) { }

        // 4스테이지를 3별로 깨면 12개. 월드 기준 10개를 넘겨 보상을 받는다.
        [Fact]
        public async Task WorldRewardStar_Succeeds_Test()
        {
            await CreateDummyPlayerAsync();

            foreach (var stageNum in StageNumList)
            {
                var res = await Api.PostAsync<WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(
                    new WorldFinishStageFirstRequestPacket(
                        worldnum: WorldNum,
                        stagenum: stageNum,
                        star: 3,
                        rewardvaluelist: [new ObjValue(EObjType.FREE_CASH, 0, ThreeStarReward)]));

                Assert.True(res.Info.ResultCode == (int)EErrorCode.OK,
                    "스테이지 " + stageNum + " 클리어 실패: " + res.Info.ResultCode + " " + res.Info.ResultMsg);
                Assert.Equal(3, res.WorldStage.Star);
            }

            // 별 12개 >= 기준 10개. 보상은 RewardStarCashList[0] = 300
            var reward = await Api.PostAsync<WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(
                new WorldRewardStarRequestPacket(
                    worldnum: WorldNum,
                    befrewardstar: 0,
                    aftrewardstar: 1,
                    totalstar: 12,
                    rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 300)));

            Assert.True(reward.Info.ResultCode == (int)EErrorCode.OK,
                "별 보상 실패: " + reward.Info.ResultCode + " " + reward.Info.ResultMsg);
            Assert.Equal(1, reward.World.RecvStarReward);
            Assert.Contains(reward.ChgObjList, x => x.Type == EObjType.FREE_CASH);

            // 두 번째 수령은 막혀야 한다
            var again = await Api.PostAsync<WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(
                new WorldRewardStarRequestPacket(
                    worldnum: WorldNum,
                    befrewardstar: 0,
                    aftrewardstar: 1,
                    totalstar: 12,
                    rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 300)));
            Assert.True(again.Info.ResultCode != (int)EErrorCode.OK, "이미 받은 별 보상이 또 나갔다");
        }

        // 별이 모자라면 막혀야 한다
        [Fact]
        public async Task WorldRewardStar_NotEnoughStar_Blocked_Test()
        {
            await CreateDummyPlayerAsync();

            // 한 스테이지만 3별 -> 3개 < 10개
            var clear = await Api.PostAsync<WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(
                new WorldFinishStageFirstRequestPacket(
                    worldnum: WorldNum,
                    stagenum: StageNumList[0],
                    star: 3,
                    rewardvaluelist: [new ObjValue(EObjType.FREE_CASH, 0, ThreeStarReward)]));
            Assert.True(clear.Info.ResultCode == (int)EErrorCode.OK, "클리어 실패: " + clear.Info.ResultMsg);

            var reward = await Api.PostAsync<WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(
                new WorldRewardStarRequestPacket(
                    worldnum: WorldNum,
                    befrewardstar: 0,
                    aftrewardstar: 1,
                    totalstar: 3,
                    rewardvalue: new ObjValue(EObjType.FREE_CASH, 0, 300)));

            Assert.True(reward.Info.ResultCode != (int)EErrorCode.OK, "별이 모자란데 보상이 나갔다");
            Assert.True(reward.Info.ResultMsg != null && reward.Info.ResultMsg.Contains("NOT_ENOUGH_TOTAL_STAR"),
                "다른 이유로 막혔다: " + reward.Info.ResultMsg);
        }

        // 반복 클리어는 이미 받은 별 다음부터만 준다
        [Fact]
        public async Task WorldFinishStageRepeat_OnlyNewStars_Test()
        {
            await CreateDummyPlayerAsync();

            var first = await Api.PostAsync<WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(
                new WorldFinishStageFirstRequestPacket(
                    worldnum: WorldNum,
                    stagenum: StageNumList[0],
                    star: 1,
                    rewardvaluelist: [new ObjValue(EObjType.FREE_CASH, 0, 20)]));   // 0성 20 + 1성 NONE
            Assert.True(first.Info.ResultCode == (int)EErrorCode.OK, "최초 클리어 실패: " + first.Info.ResultMsg);
            Assert.Equal(1, first.WorldStage.Star);

            // 1 -> 3 으로 올리면 2성(50) + 3성(100) 만
            var repeat = await Api.PostAsync<WorldFinishStageRepeatRequestPacket, WorldFinishStageRepeatResponsePacket>(
                new WorldFinishStageRepeatRequestPacket(
                    worldnum: WorldNum,
                    stagenum: StageNumList[0],
                    star: 3,
                    rewardvaluelist: [new ObjValue(EObjType.FREE_CASH, 0, 150)]));
            Assert.True(repeat.Info.ResultCode == (int)EErrorCode.OK, "반복 클리어 실패: " + repeat.Info.ResultMsg);
            Assert.Equal(3, repeat.WorldStage.Star);
        }
    }
}

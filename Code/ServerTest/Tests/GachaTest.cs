using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// gacha/normal, schedule/load 테스트
    ///
    /// Proto 데이터 기준:
    ///   Schedule 1001001: GACHA, ContentEndTime=2026-03-20 → 현재(2026-03-27) 기준 만료됨
    ///   GachaSchedule 1001001: 1회=POINT_C_GACHA_NORMAL:1 또는 TOTAL_CASH:100
    ///
    /// NOTE: 가챠 성공 케이스는 활성화된 스케줄이 필요합니다.
    ///       Schedule.csv의 ContentEndTime을 현재 날짜 이후로 업데이트해야 합니다.
    /// </summary>
    public class GachaTest : TestBase
    {
        private const int ActiveScheduleNum = 1001001;   // 현재 만료된 스케줄 (CSV 업데이트 필요)

        public GachaTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task ScheduleLoad_Test()
        {
            await CreateDummyPlayerAsync();

            // [성공] 스케줄 목록 로드
            {
                var res = await Api.PostAsync<ScheduleLoadRequestPacket, ScheduleLoadResponsePacket>(
                    new ScheduleLoadRequestPacket());

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.ScheduleList);
                // CenterDb InMemory에 스케줄이 없을 수 있음 (빈 목록도 정상)
            }

            // [실패] 세션 없이 요청
            {
                var res = await Api.PostAsync<ScheduleLoadRequestPacket, ScheduleLoadResponsePacket>(
                    new ScheduleLoadRequestPacket(), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }

        [Fact]
        public async Task GachaNormal_Test()
        {
            await CreateDummyPlayerAsync();

            // 가챠 포인트 지급 (TOTAL_CASH는 감소 전용 복합타입이므로 FREE_CASH로 지급)
            var cheatRes = await Api.PostAsync<CheatRewardRequestPacket, CheatRewardResponsePacket>(
                new CheatRewardRequestPacket(new List<ObjValue>
                {
                    new ObjValue(EObjType.POINT_C_GACHA_NORMAL, 0, 100000),
                    new ObjValue(EObjType.FREE_CASH, 0, 100000)
                }));
            Assert.Equal((int)EErrorCode.OK, cheatRes.Info.ResultCode);

            // [실패] 존재하지 않는 스케줄 번호
            {
                var res = await Api.PostAsync<GachaNormalRequestPacket, GachaNormalResponsePacket>(
                    new GachaNormalRequestPacket(
                        schedulenum: 9999999,
                        cnt: 1,
                        costobj: new CostObjPacket { Type = EObjType.POINT_C_GACHA_NORMAL, Num = 0, Amount = 1 }
                    ));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 만료된 스케줄로 요청 (Schedule.csv ContentEndTime이 과거임)
            // NOTE: Schedule.csv를 업데이트하면 이 케이스는 성공 케이스로 변경 필요
            {
                var res = await Api.PostAsync<GachaNormalRequestPacket, GachaNormalResponsePacket>(
                    new GachaNormalRequestPacket(
                        schedulenum: ActiveScheduleNum,
                        cnt: 1,
                        costobj: new CostObjPacket { Type = EObjType.POINT_C_GACHA_NORMAL, Num = 0, Amount = 1 }
                    ));

                // 스케줄 만료 → 에러 기대
                // Schedule.csv의 ContentEndTime을 미래로 수정 후 아래 성공 케이스로 교체
                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 세션 없이 요청
            {
                var res = await Api.PostAsync<GachaNormalRequestPacket, GachaNormalResponsePacket>(
                    new GachaNormalRequestPacket(
                        schedulenum: ActiveScheduleNum,
                        cnt: 1,
                        costobj: new CostObjPacket { Type = EObjType.POINT_C_GACHA_NORMAL, Num = 0, Amount = 1 }
                    ), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // TODO: Schedule.csv의 ContentEndTime을 미래 날짜로 업데이트 후 아래 케이스 활성화
            // [성공] 1회 가챠 (POINT_C_GACHA_NORMAL 1개 소모)
            // {
            //     var res = await Api.PostAsync<GachaNormalRequestPacket, GachaNormalResponsePacket>(
            //         new GachaNormalRequestPacket(
            //             schedulenum: ActiveScheduleNum,
            //             cnt: 1,
            //             costobj: new CostObjPacket { Type = EObjType.POINT_C_GACHA_NORMAL, Num = 0, Amount = 1 }
            //         ));
            //     Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
            //     Assert.NotNull(res.GachaResultList);
            //     Assert.Single(res.GachaResultList);
            // }
        }
    }
}

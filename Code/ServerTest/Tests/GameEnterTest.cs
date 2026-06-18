using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// game/enter 테스트
    /// - 세션 필요 (AUTHORIZED), 플레이어 없어도 됨
    /// </summary>
    public class GameEnterTest : TestBase
    {
        public GameEnterTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task GameEnter_Test()
        {
            var deviceKey = Guid.NewGuid().ToString();
            var signUpRes = await Api.PostAsync<AuthSignUpRequestPacket, AuthSignUpResponsePacket>(
                new AuthSignUpRequestPacket(deviceKey));
            Assert.Equal((int)EErrorCode.OK, signUpRes.Info.ResultCode);
            var sessionKey = signUpRes.Result.SessionKey;
            Api.SetSession(sessionKey);

            // [성공] 최초 Enter → 플레이어 생성
            {
                var res = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket());

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.Player);
                Assert.True(res.Player.SfId > 0);
            }

            // [성공] 재접속 Enter → 기존 플레이어 로드 (Id 동일)
            {
                var res1 = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket());
                var res2 = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket());

                Assert.Equal((int)EErrorCode.OK, res1.Info.ResultCode);
                Assert.Equal((int)EErrorCode.OK, res2.Info.ResultCode);
                Assert.Equal(res1.Player.SfId, res2.Player.SfId);
            }

            // [실패] 세션키 없이 호출
            {
                var res = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket(), sessionKey: "");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 유효하지 않은 세션키
            {
                var res = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket(), sessionKey: "INVALID_SESSION_KEY");

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// game/change-name 테스트
    /// - 이름 중복 검사는 전 샤드 조회를 탄다.
    /// </summary>
    public class ChangeNameTest : TestBase
    {
        public ChangeNameTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task ChangeName_Test()
        {
            var name = $"Player_{Guid.NewGuid():N}"[..16];

            // [성공] 첫 계정이 이름을 바꾼다
            {
                await SignUpAndEnterAsync();

                var res = await Api.PostAsync<GameChangeNameRequestPacket, GameChangeNameResponsePacket>(
                    new GameChangeNameRequestPacket { PlayerName = name });

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.Equal(name, res.PlayerName);
            }

            // [성공] 저장됐는지 재입장으로 확인
            {
                var res = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                    new GameEnterRequestPacket());

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.Equal(name, res.Player.ProfileName);
            }

            // [실패] 다른 계정이 같은 이름을 쓰면 막힌다
            {
                await SignUpAndEnterAsync();

                var res = await Api.PostAsync<GameChangeNameRequestPacket, GameChangeNameResponsePacket>(
                    new GameChangeNameRequestPacket { PlayerName = name });

                Assert.Equal((int)EErrorCode.GAME_CHANGE_NAME_EXIST_NAME, res.Info.ResultCode);
            }
        }

        private async Task SignUpAndEnterAsync()
        {
            var signUpRes = await Api.PostAsync<AuthSignUpRequestPacket, AuthSignUpResponsePacket>(
                new AuthSignUpRequestPacket(Guid.NewGuid().ToString()));
            Assert.Equal((int)EErrorCode.OK, signUpRes.Info.ResultCode);

            Api.SetSession(signUpRes.Result.SessionKey);

            var enterRes = await Api.PostAsync<GameEnterRequestPacket, GameEnterResponsePacket>(
                new GameEnterRequestPacket());
            Assert.Equal((int)EErrorCode.OK, enterRes.Info.ResultCode);
        }
    }
}

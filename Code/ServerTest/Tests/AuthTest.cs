using Proto;
using Protocol;
using Xunit;

namespace ServerTest.Tests
{
    /// <summary>
    /// auth/sign-up, auth/sign-in 테스트
    /// </summary>
    public class AuthTest : TestBase
    {
        public AuthTest(GameServerFactory factory) : base(factory) { }

        [Fact]
        public async Task SignUp_Test()
        {
            // [성공] 새 디바이스로 회원가입
            {
                var deviceKey = Guid.NewGuid().ToString();
                var res = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                    new AuthSignUpReqPacket(deviceKey));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.Result);
                Assert.False(string.IsNullOrEmpty(res.Result.SessionKey));
                Assert.False(string.IsNullOrEmpty(res.Result.ChannelKey));
            }

            // [성공] 같은 디바이스키로 다시 회원가입 → 기존 계정 반환
            {
                var deviceKey = Guid.NewGuid().ToString();

                var res1 = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                    new AuthSignUpReqPacket(deviceKey));
                var res2 = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                    new AuthSignUpReqPacket(deviceKey));

                Assert.Equal((int)EErrorCode.OK, res1.Info.ResultCode);
                Assert.Equal((int)EErrorCode.OK, res2.Info.ResultCode);
                // 같은 채널키 반환 (동일 계정)
                Assert.Equal(res1.Result.ChannelKey, res2.Result.ChannelKey);
            }

            // [성공] 응답에 환경 정보 포함
            {
                var res = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                    new AuthSignUpReqPacket(Guid.NewGuid().ToString()));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.False(string.IsNullOrEmpty(res.Result.AccountEnv));
            }
        }

        [Fact]
        public async Task SignIn_Test()
        {
            // 미리 계정 생성
            var deviceKey = Guid.NewGuid().ToString();
            var signUpRes = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                new AuthSignUpReqPacket(deviceKey));
            Assert.Equal((int)EErrorCode.OK, signUpRes.Info.ResultCode);
            var channelKey = signUpRes.Result.ChannelKey;

            // [성공] 유효한 채널키로 로그인
            {
                var res = await Api.PostAsync<AuthSignInReqPacket, AuthSignInResPacket>(
                    new AuthSignInReqPacket(channelKey));

                Assert.Equal((int)EErrorCode.OK, res.Info.ResultCode);
                Assert.NotNull(res.Result);
                Assert.False(string.IsNullOrEmpty(res.Result.SessionKey));
                Assert.Equal(channelKey, res.Result.ChannelKey);
            }

            // [실패] 존재하지 않는 채널키
            {
                var res = await Api.PostAsync<AuthSignInReqPacket, AuthSignInResPacket>(
                    new AuthSignInReqPacket("INVALID_CHANNEL_KEY_XXXX"));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }

            // [실패] 빈 채널키
            {
                var res = await Api.PostAsync<AuthSignInReqPacket, AuthSignInResPacket>(
                    new AuthSignInReqPacket(""));

                Assert.NotEqual((int)EErrorCode.OK, res.Info.ResultCode);
            }
        }
    }
}

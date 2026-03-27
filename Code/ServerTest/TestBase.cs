using System.Net.Http;
using System.Text;
using System.Text.Json;
using Proto;
using Protocol;
using Xunit;

namespace ServerTest
{
    /// <summary>
    /// 모든 테스트 클래스의 베이스.
    /// - [Collection("GameServer")] 방식으로 단일 서버 인스턴스 공유
    /// - TestApiClient를 통해 HTTP 요청 전송
    /// </summary>
    [Collection("GameServer")]
    public abstract class TestBase
    {
        protected readonly TestApiClient Api;

        protected TestBase(GameServerFactory factory)
        {
            var httpClient = factory.CreateClient();
            Api = new TestApiClient(httpClient);
        }

        /// <summary>
        /// SignUp + GameEnter로 더미 플레이어를 생성하고 sessionKey를 반환
        /// </summary>
        protected async Task<string> CreateDummyPlayerAsync(string deviceKey = null)
        {
            deviceKey ??= Guid.NewGuid().ToString();

            var signUpRes = await Api.PostAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(
                new AuthSignUpReqPacket(deviceKey));
            Assert.Equal((int)EErrorCode.OK, signUpRes.Info.ResultCode);

            var sessionKey = signUpRes.Result.SessionKey;
            Api.SetSession(sessionKey);

            var enterRes = await Api.PostAsync<GameEnterReqPacket, GameEnterResPacket>(
                new GameEnterReqPacket());
            Assert.Equal((int)EErrorCode.OK, enterRes.Info.ResultCode);

            return sessionKey;
        }

        protected bool IsOk(IResPacket res) => res.Info.ResultCode == (int)EErrorCode.OK;
        protected bool IsError(IResPacket res) => res.Info.ResultCode != (int)EErrorCode.OK;
    }

    /// <summary>
    /// WebApplicationFactory의 HttpClient를 이용한 JSON 기반 API 클라이언트
    /// </summary>
    public class TestApiClient
    {
        private readonly HttpClient _httpClient;
        private string _sessionKey = string.Empty;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };

        public TestApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetSession(string key) => _sessionKey = key;
        public void ClearSession() => _sessionKey = string.Empty;

        public async Task<TRes> PostAsync<TReq, TRes>(TReq req, string sessionKey = null)
            where TReq : IReqPacket, new()
            where TRes : IResPacket, new()
        {
            req.Info ??= new ReqInfoPacket { Seq = 0 };

            var key = sessionKey ?? _sessionKey;
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var protocolName = req.GetProtocolName();
            var url = $"/rpc/{protocolName}?sessionkey={key}&timestamp={ts}";

            var json = JsonSerializer.Serialize(req, req.GetType(), JsonOpts);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            var resBytes = await response.Content.ReadAsByteArrayAsync();
            var resJson = Encoding.UTF8.GetString(resBytes);

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<TRes>(resJson, JsonOpts)
                    ?? throw new Exception($"Deserialize failed: {resJson}");
            }

            // 에러 응답: ErrorResponsePacket에서 Info를 꺼내서 TRes에 세팅
            var errorRes = JsonSerializer.Deserialize<ErrorResponsePacket>(resJson, JsonOpts);
            var res = new TRes();
            res.Info = errorRes?.Info ?? new ResInfoPacket { ResultCode = (int)EErrorCode.NO_HANDLING_ERROR, ResultMsg = resJson };
            return res;
        }
    }
}

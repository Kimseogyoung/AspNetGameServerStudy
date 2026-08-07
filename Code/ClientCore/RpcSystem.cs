using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Proto;
using Protocol;

namespace ClientCore
{
    public class RpcSystem
    {
        public string SessionId => _sessionKey;
        public string DeviceKey => _deviceKey;
        public string Host => _host;

        public void Init(string host, string contentType, TimeSpan timeoutSpan)
        {
            _host = host.Trim('/');
            _contentType = contentType;
            _httpClient = new HttpClient { Timeout = timeoutSpan };
        }

        public void Clear()
        {
            _seq = 0;
            _sessionKey = string.Empty;
            _deviceKey = string.Empty;
            _prevTimestamp = 0;
            _host = string.Empty;
            _contentType = string.Empty;
            _httpClient = null;
        }

        public void SetSessionKey(string key)
        {
            _sessionKey = key;
        }

        public void SetDeviceKey(string key)
        {
            _deviceKey = key;
        }


        public async Task<RES> RequestAsync<REQ, RES>(REQ req)
            where REQ : IRequestPacket, new()
            where RES : IResponsePacket, new()
        {
            req.Info = new RequestInfoPacket
            {
                Seq = ++_seq
            };

            // 요청 URL
            var protocolName = req.GetProtocolName();
            var url = $"{_host}/rpc/{protocolName}";
            var fullUrl = MakeQueryString(url);

            // 요청 데이터 (JSON 형식)
            var reqBodyArr = ByteArrSerialize<REQ>(req);
            //var content = new StringContent(reqBodyString, Encoding.UTF8, _contentType);

            // ByteArrayContent 생성
            using var content = new ByteArrayContent(reqBodyArr);

            // Content-Type 헤더 설정
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_contentType);

            // POST 요청 보내기
            try
            {
                var response = await _httpClient.PostAsync(fullUrl, content);

                // 응답 처리
                if (response.IsSuccessStatusCode)
                {
                    var resContentType = response.Content.Headers.ContentType.MediaType.ToString();
                    var responseByteArr = await response.Content.ReadAsByteArrayAsync();
                    var res = Deserialize<RES>(resContentType, responseByteArr);

                    var json = JsonSerializer.Serialize(res);
                    Console.WriteLine("응답: " + json);
                    return res;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    // TODO: 예외처리
                    var resContentType = response.Content.Headers.ContentType.MediaType.ToString();
                    Console.WriteLine($"에러: {response.StatusCode}");
                    var responseByteArr = await response.Content.ReadAsByteArrayAsync();
                    var errorRes = Deserialize<ErrorResponsePacket>(resContentType, responseByteArr);
                    var res = new RES();
                    res.Info = errorRes.Info;

                    var json = JsonSerializer.Serialize(res);
                    Console.WriteLine("응답: " + json);
                    return res;
                }
                else
                {
                    return MakeErrorResult<RES>(EErrorCode.NO_HANDLING_ERROR, $"{response.StatusCode}Code");
                }
            }
            catch (Exception exc)
            {
                if (exc.InnerException is SocketException)
                {
                    return MakeErrorResult<RES>(EErrorCode.SERVER_DOWN, exc.Message);
                }

                if (exc is TaskCanceledException)
                {
                    return MakeErrorResult<RES>(EErrorCode.TIMEOUT, exc.Message);
                }

                var innerDesc = exc.InnerException != null
                    ? $"{exc.InnerException.GetType().Name}:{exc.InnerException.Message}"
                    : "None";
                return MakeErrorResult<RES>(EErrorCode.NO_HANDLING_ERROR, $"Exception Msg({exc.Message}) InnerException({innerDesc})");
            }

        }

        private static RES MakeErrorResult<RES>(EErrorCode code, string msg) where RES : IResponsePacket, new()
        {
            var res = new RES();
            res.Info.ResultCode = (int)code;
            res.Info.ResultMsg = msg;
            return res;
        }

        private byte[] ByteArrSerialize<REQ>(REQ obj)
        {
            switch (_contentType)
            {
                case MsgProtocol.JsonContentType:
                    var json = JsonSerializer.Serialize<REQ>(obj);
                    var jsonByteArray = Encoding.UTF8.GetBytes(json);
                    return jsonByteArray;
                case MsgProtocol.ProtoBufContentType:
                    byte[] serializedData;
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, obj);
                        serializedData = ms.ToArray();
                    }
                    return serializedData;
                default:
                    break;
            }
            return null;
        }

        private RES Deserialize<RES>(string contentType, byte[] byteArr) where RES : IResponsePacket, new()
        {
            var res = new RES();
            res.Info.ResultCode = (int)EErrorCode.NO_HANDLING_ERROR;
            res.Info.ResultMsg = "FAILED_DESERIALIZE";
            switch (contentType)
            {
                case MsgProtocol.JsonContentType:
                    var stringData = Encoding.UTF8.GetString(byteArr);
                    res = JsonSerializer.Deserialize<RES>(stringData, Opts);
                    break;
                case MsgProtocol.ProtoBufContentType:
                    {
                        using var ms = new MemoryStream(byteArr);
                        res = ProtoBuf.Serializer.Deserialize<RES>(ms);
                    }
                    break;
                default:
                    break;
            }

            if (res == null)
            {
                throw new Exception("FAILED_DESERIALIZE ~~~~~");
            }

            return res;
        }

        private string MakeQueryString(string url)
        {
            var timestamp = GetTimestamp();
            var fullUrl = $"{url}?sessionkey={_sessionKey}&devicekey={_deviceKey}&timestamp={timestamp}&seq={_seq}";
            return fullUrl;
        }

        private long GetTimestamp()
        {
            var nowTime = DateTime.UtcNow;
            var timestmap = ((DateTimeOffset)nowTime).ToUnixTimeMilliseconds();
            if (_prevTimestamp == timestmap)
            {
                timestmap += 1;
            }

            _prevTimestamp = timestmap;
            return timestmap;
        }

        private long _prevTimestamp = 0;
        private long _seq = 0;
        private string _sessionKey = string.Empty;
        private string _deviceKey = string.Empty;

        private string _host = string.Empty;
        private string _contentType = string.Empty;
        private HttpClient _httpClient;

        public readonly static JsonSerializerOptions Opts = new JsonSerializerOptions
        {
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() // .net 8.0 이상부터 설정 필요.
            // NOTE:  Ops에서 필드 전부 표시
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

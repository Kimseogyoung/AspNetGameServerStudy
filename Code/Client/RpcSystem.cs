using Microsoft.EntityFrameworkCore.Metadata;
using NLog;
using Proto;
using Protocol;
using SharpYaml.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Client
{
    public class RpcSystem
    {
        public string SessionId => _sessionKey;

        public void Init(string host, string contentType)
        {
            _host = host.Trim('/');
            _contentType = contentType;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        }

        public void Clear()
        {
            _seq = 0;
            _sessionKey = "";
            _prevTimestamp = 0;
            _host = "";
            _contentType = "";
            _httpClient = null;
        }

        public void SetSessionKey(string key)
        {
            _sessionKey = key;
        }

        public async Task<RES> RequestAsync<REQ, RES>(REQ req)
            where REQ : IReqPacket, new()
            where RES : IResPacket, new()
        {
            req.Info = new ReqInfoPacket
            {
                Seq = 0
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
            else if(response.StatusCode == HttpStatusCode.InternalServerError)
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
                throw new Exception("ERROR");
            }
            else
            {
                var res = new RES();
                res.Info.ResultCode = (int)EErrorCode.NO_HANDLING_ERROR;
                res.Info.ResultMsg = $"{response.StatusCode}Code";
                return res;
            }
        }

        private string StringSerialize<REQ>(REQ obj)
        {
            switch (_contentType)
            {
                case MsgProtocol.JsonContentType:
                    var json = JsonSerializer.Serialize<REQ>(obj, Opts);
                    return json;
                case MsgProtocol.ProtoBufContentType:
                    byte[] serializedData;
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, obj);
                        serializedData = ms.ToArray();
                    }
                    var protoBufStr = Encoding.UTF8.GetString(serializedData);
                    return protoBufStr;
                default:
                    break;
            }
            return string.Empty;
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

        private RES Deserialize<RES>(string contentType, byte[] byteArr) where RES : IResPacket, new()
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

            if(res == null)
            {
                throw new Exception("FAILED_DESERIALIZE ~~~~~");
            }

            return res;
        }

        private string MakeQueryString(string url)
        {
            var timestamp = GetTimestamp();
            var fullUrl = $"{url}?sessionkey={_sessionKey}&timestamp={timestamp}";
            return fullUrl;
        }

        private long GetTimestamp()
        {
            var nowTime = DateTime.UtcNow;
            var timestmap = ((DateTimeOffset)nowTime).ToUnixTimeMilliseconds();
            if ( _prevTimestamp == timestmap)
            {
                timestmap += 1;
            }

            _prevTimestamp = timestmap;
            return timestmap;
        }

        private long _prevTimestamp = 0;
        private long _seq = 0;
        private string _sessionKey = string.Empty;

        private string _host = string.Empty;
        private string _contentType = string.Empty;
        private HttpClient _httpClient;

        private static readonly DateTime s_timestampBaseTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public readonly static JsonSerializerOptions Opts = new JsonSerializerOptions
        {
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() // .net 8.0 이상부터 설정 필요.
            // NOTE:  Ops에서 필드 전부 표시
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

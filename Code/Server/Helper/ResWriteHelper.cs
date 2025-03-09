using NLog;
using Protocol;
using Server.Formatter;
using WebStudyServer.GAME;

namespace WebStudyServer.Helper
{
    public static class ResWriteHelper
    {
        // NOTE: 강제로 응답을 작성해주는 경우 호출
        public static async Task WriteResponseBodyAsync(HttpContext httpContext, object body, Type type, int statusCode = StatusCodes.Status200OK)
        {
            httpContext.Response.StatusCode = statusCode;

            var contentType = GetOutputContentType(httpContext);
            httpContext.Response.ContentType = contentType;

            try
            {
                switch (contentType)
                {
                    case MsgProtocol.JsonContentType:
                        {
                            var text = JsonDataSerializer.SerializeStr(body);
                            await httpContext.Response.WriteAsync(text);
                            break;
                        }
                    case MsgProtocol.ProtoBufContentType:
                        {
                            await ProtoBufOutputFormatter.WriteResponseBodyAsync2(httpContext, body);
                            break;
                        }
                    //case MsgProtocol.MsgPackProtocol:
                    //    var bytes = MessagePackSerializer.Serialize(type, body, _msgPackOptions);
                    //    await httpContext.Response.Body.WriteAsync(bytes);
                    //    break;
                    //case MsgProtocol.MsgPackNoLz4Protocol:
                    //    var bytes2 = MessagePackSerializer.Serialize(type, body, _msgPackNoLz4Options);
                    //    await httpContext.Response.Body.WriteAsync(bytes2);
                    //    break;

                    default:
                        {
                            var text = JsonDataSerializer.SerializeStr(body);
                            await httpContext.Response.WriteAsync(text);
                            _logger.Error("UNKNOWN_MSG_PROTOCOL ContentType{ContentType})", contentType, contentType);
                            return;
                        }
                }
            }
            catch (ObjectDisposedException exc)
            {
                _logger.Error("FAILED_WRITE_BODY ErrMsg({Msg}) CallStack({CallStack})", exc.Message, exc.StackTrace);
            }

        }

        //public static async Task WriteResponseBodyByteArrAsync(HttpContext httpContext, byte[] bodyByteArr, int statusCode = StatusCodes.Status200OK)
        //{
        //    var outContentType = GetOutputContentType(httpContext);
        //    httpContext.Response.ContentType = outContentType;
        //    httpContext.Response.StatusCode = statusCode; // Write 전에 StatusCode 넣어야함.
        //    await httpContext.Response.Body.WriteAsync(bodyByteArr);
        //}

        //public static byte[] GetSerializedBodyByteArr(HttpContext httpContext, object body, Type type)
        //{
        //    var outContentType = GetOutputContentType(httpContext);
        //    switch (outContentType)
        //    {
        //        case MsgProtocol.JsonContentType:
        //            var jsonData = SerializeHelper.JsonSerialize(body);
        //            return UTF8Encoding.UTF8.GetBytes(jsonData);
        //        case MsgProtocol.ProtoBufContentType:

        //            break;
        //        case MsgProtocol.MsgPackProtocol:
        //            var bytes = MessagePackSerializer.Serialize(type, body, _msgPackOptions);
        //            return bytes;
        //        case MsgProtocol.MsgPackNoLz4Protocol:
        //        default:
        //            throw new GameException("NO_HANDLING_CONTENT_TYPE");
        //    }
        //}

        public static string GetOutputContentType(HttpContext httpContext)
        {
            if (!string.IsNullOrEmpty(APP.Cfg.ForceContentType)) // 강제설정값이 있음.
            {
                return APP.Cfg.ForceContentType;
            }

            if (httpContext.Request.Query.TryGetValue("Out", out var outContentType) && MsgProtocol.ContentTypeList.Contains(outContentType))
            {
                return outContentType;
            }

            // 강제 형식
            return APP.Cfg.ForceContentType;
        }

        //private static string GetClientIpAddress(HttpContext httpContext)
        //{
        //    var clientIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        //    string forwardedForHeader = httpContext.Request.Headers["X-Forwarded-For"];
        //    if (!string.IsNullOrEmpty(forwardedForHeader))
        //    {
        //        // 여러 개의 IP 주소가 콤마로 구분되어 있을 수 있음
        //        var forwardedIps = forwardedForHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
        //        if (forwardedIps.Length > 0)
        //        {
        //            // 첫 번째 IP 주소를 사용 (가장 처음에 있는 것이 가장 원래의 클라이언트 주소)
        //            clientIpAddress = forwardedIps[0].Trim();
        //        }
        //    }
        //    return clientIpAddress;
        //}

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    }
}

using Microsoft.AspNetCore.Mvc.Formatters;
using NLog;
using NLog.Targets;
using Protocol;
using Server.Formatter;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebStudyServer.Extension;
using WebStudyServer.Helper;

namespace WebStudyServer
{
    public class CustomOutputFormatter : TextOutputFormatter
    {
        public CustomOutputFormatter()
        {
            foreach (var contentType in MsgProtocol.ContentTypeList)
            {
                IOutputFormatter outputFormatter;
                switch (contentType)
                {
                    case MsgProtocol.JsonContentType:
                        outputFormatter = new SystemTextJsonOutputFormatter(JsonDataSerializer.Opts);
                        break;
                    case MsgProtocol.ProtoBufContentType:
                        outputFormatter = new ProtoBufOutputFormatter();
                        break;
                    default:
                        _logger.Error("NOT_SUPPORT_OUTPUT_PROTOCOL MsgProtocol({MsgProtocol})", contentType);
                        continue;
                }

                // NOTE: 지원 타입을 명시적으로 작성 (아무것도 추가 안하면 에러 발생)
                _formatterDict.Add(contentType, outputFormatter);
                SupportedMediaTypes.Add(contentType);
            }

            if (_formatterDict.Count == 0)
            {
                _logger.Error("OUTPUT_FORMATTER_IS_EMPTY");
                return;
            }

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }


        // NOTE: 모든 요청에 대해서 자동 호출됨
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var outputType = ResWriteHelper.GetOutputContentType(context.HttpContext);
            // ContentType에 따라 데이터를 읽고 처리
            if (!_formatterDict.TryGetValue(outputType, out var formatter))
            {
                _logger.Error("NOT_FOUND_VALID_OUPUT_CONTENT_TYPE ContentType{ContentType}", outputType); // TODO: Output Formatter 에러
                return;
            }

            try
            {
                context.ContentType = outputType; // WriteAsync에서 context.ContentType 읽어서  httpContext.Response.ContentType에 넣어줌
                await formatter.WriteAsync(context);
            }
         /*   catch (MessagePackSerializationException ex)
            {
                if (ex.InnerException is OperationCanceledException)
                {
                    // Handle the cancellation specifically
                    _logger.Warning("FAILED_WRITE_RESPONSE_BODY ErrorMsg({ErrorMsg}) Stack({Stack}) IsCancellationRequested({IsCancellationRequested})"
                        , ex.InnerException.ToString(), ex.StackTrace, context.HttpContext.RequestAborted.IsCancellationRequested);
                    throw new CancelReqException(context.HttpContext.Request.Path);
                }
                else
                {
                    throw;
                }
            }*/
            // NOTE: 중간에 클라 요청이 취소된 경우
            catch (OperationCanceledException ex) when (context.HttpContext.RequestAborted.IsCancellationRequested)
            {
                // Handle the cancellation specifically
                _logger.Warn("FAILED_WRITE_RESPONSE_BODY ErrorMsg({ErrorMsg}) Stack({Stack}) IsCancellationRequested({IsCancellationRequested})"
                    , ex.InnerException.ToString(), ex.StackTrace, context.HttpContext.RequestAborted.IsCancellationRequested);
                throw new CancelReqException(context.HttpContext.Request.Path);
            }
            catch (OperationCanceledException ex)
            {
                // Handle the cancellation specifically
                _logger.Warn("FAILED_WRITE_RESPONSE_BODY Operation was canceled. ErrorMsg({ErrorMsg}) Stack({Stack})"
                    , ex.InnerException.ToString(), ex.StackTrace);
                throw new CancelReqException(context.HttpContext.Request.Path);
            }
        }
        private static string GetContentTypeByHeader(HttpContext httpContext)
        {
            var fullContentType = httpContext.Request.ContentType;
            return fullContentType.Split(";")[0];
        }

        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, IOutputFormatter> _formatterDict = new();
    }
}

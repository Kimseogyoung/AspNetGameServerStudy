using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog;
using Protocol;
using System.Text;
using ProtoBuf.AspNetCore;
namespace WebStudyServer
{
    public class CustomInputFormatter : TextInputFormatter
    {
        public CustomInputFormatter()
        {
            foreach (var contentType in MsgProtocol.ContentTypeList)
            {
                IInputFormatter inputFormatter;
                switch (contentType)
                {
                    case MsgProtocol.JsonContentType:
                        var jsonLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SystemTextJsonInputFormatter>();

                        var jsonOptions = new JsonOptions();
                        inputFormatter = new SystemTextJsonInputFormatter(jsonOptions, jsonLogger);
                        break;
                    case MsgProtocol.ProtoBufContentType:
                        inputFormatter = new ProtoBufInputFormatter();
                        break;
                    default:
                        _logger.Error("NOT_SUPPORT_INPUT_PROTOCOL MsgProtocol({MsgProtocol})", contentType);
                        continue;
                }

                _formatterDict.Add(contentType, inputFormatter);

                // NOTE: 지원 타입을 명시적으로 작성 (아무것도 추가 안하면 에러 발생)
                SupportedMediaTypes.Add(contentType);
            }

            if (_formatterDict.Count == 0)
            {
                _logger.Error("INPUT_FORMATTER_IS_EMPTY");
                return;
            }

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        public static string GetContentTypeByHeader(HttpContext httpContext)
        {
            var fullContentType = httpContext.Request.ContentType;
            return fullContentType.Split(";")[0];
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var dataContentType = GetContentTypeByHeader(context.HttpContext);

            // ContentType에 따라 데이터를 읽고 처리
            if (!_formatterDict.TryGetValue(dataContentType, out var formatter))
            {
                _logger.Error("NOT_FOUND_VALID_INPUT_CONTENT_TYPE ContentType{ContentType}", dataContentType); // TODO: Input Formatter 에러
                return InputFormatterResult.Failure();
            }

            return await formatter.ReadAsync(context);
        }

        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, IInputFormatter> _formatterDict = new();

    }
}

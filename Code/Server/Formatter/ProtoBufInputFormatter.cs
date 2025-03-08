using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using Server.Formatter;

namespace WebStudyServer
{
    public class ProtoBufInputFormatter : InputFormatter
    {

        public ProtoBufInputFormatter()
        {
            SupportedMediaTypes.Add(MsgProtocol.ProtoBufContentType);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var result = await _serializer.DeserializeAsync(context.ModelType, request.Body);  

            // Return the result
            return await InputFormatterResult.SuccessAsync(result).ConfigureAwait(false);
        }

        private ProtoBufDataSerializer _serializer = new ProtoBufDataSerializer();
    }
}

using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using ProtoBuf;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

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

            using var ms = new MemoryStream();
            await request.Body.CopyToAsync(ms);
            ms.Position = 0;

            //var byteArr = ms.ToArray();

            // Asynchronously read the body into a string
            /*     var body = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);
                 var byteArr = Encoding.UTF8.GetBytes(body);
                 using var ms = new MemoryStream(byteArr);*/
            var result = ProtoBuf.Serializer.Deserialize(context.ModelType, ms);

            // Return the result
            return await InputFormatterResult.SuccessAsync(result).ConfigureAwait(false);

            /*  var result = Serializer.Deserialize(context.ModelType, request.Body);
              return await InputFormatterResult.SuccessAsync(result).ConfigureAwait(false);*/
        }
    }
}

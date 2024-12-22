using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using ProtoBuf;
using Microsoft.AspNetCore.Http;

namespace WebStudyServer
{
    public class ProtoBufOutputFormatter : OutputFormatter
    {
        public ProtoBufOutputFormatter()
        {
            SupportedMediaTypes.Add(MsgProtocol.ProtoBufContentType);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            await WriteResponseBodyAsync2(context.HttpContext, context.Object);
         /*   var objectType = context.ObjectType == null || context.ObjectType == typeof(object) ? context.Object.GetType() : context.ObjectType;

            var writer = context.HttpContext.Response.BodyWriter;
            if (writer == null)
            {
                Serializer.Serialize(context.HttpContext.Response.Body, context.Object);
            }

            var memory = writer.GetMemory(); // 메모리 버퍼 할당
            using (var stream = new MemoryStream(memory.Length))
            {
                // MemoryStream에 직렬화
                Serializer.Serialize(stream, context.Object);
                stream.Position = 0;

                // MemoryStream 데이터를 PipeWriter에 기록
                await stream.CopyToAsync(writer.AsStream());
            }

            // 데이터 기록 완료
            await writer.FlushAsync().AsTask();*/
        }

        public static async Task WriteResponseBodyAsync2(HttpContext httoContext, object obj)
        {
            httoContext.Response.ContentType = MsgProtocol.ProtoBufContentType; 
            var writer = httoContext.Response.BodyWriter;
            if (writer == null)
            {
                Serializer.Serialize(httoContext.Response.Body, obj);
            }

            var memory = writer.GetMemory(); // 메모리 버퍼 할당
            using (var stream = new MemoryStream(memory.Length))
            {
                // MemoryStream에 직렬화
                Serializer.Serialize(stream, obj);
                stream.Position = 0;

                // MemoryStream 데이터를 PipeWriter에 기록
                await stream.CopyToAsync(writer.AsStream());
            }

            // 데이터 기록 완료
            await writer.FlushAsync().AsTask();
        }
    }
}

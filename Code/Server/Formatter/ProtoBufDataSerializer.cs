
using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using System;

namespace Server.Formatter
{
    public class ProtoBufDataSerializer : IDataSerializer
    {
        public string ContentType => MsgProtocol.ProtoBufContentType;

        public async Task<T> DeserializeAsync<T>(Stream inStream)
        {
            var type = typeof(T);
            using var ms = new MemoryStream();
            await inStream.CopyToAsync(ms);
            ms.Position = 0;

            var result = ProtoBuf.Serializer.Deserialize<T>(ms);
            return result;
        }

        public async Task<object> DeserializeAsync(Type type, Stream inStream)
        {
            using var ms = new MemoryStream();
            await inStream.CopyToAsync(ms);
            ms.Position = 0;

            var result = ProtoBuf.Serializer.Deserialize(type, ms);
            return result;
        }

        public byte[] Serialize<T>(T inObj)
        {
            if (inObj == null)
                return null;

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, inObj);
                return ms.ToArray();
            }
        }

        public async Task SerializeAsync<T>(Stream inStream, T inObj)
        {
            if (inStream == null || inObj == null)
                return;

            ProtoBuf.Serializer.Serialize(inStream, inObj);
            await inStream.FlushAsync();
        }
    }
}

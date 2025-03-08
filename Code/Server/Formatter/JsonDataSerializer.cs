
using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace Server.Formatter
{
    public class JsonDataSerializer : IDataSerializer
    {
        public string ContentType => MsgProtocol.JsonContentType;

        public byte[] Serialize<T>(T inObj)
        {
            var str = JsonSerializer.Serialize(inObj, options: _opts);
            var byteArr = Encoding.UTF8.GetBytes(str);
            return byteArr;
        }

        public async Task SerializeAsync<T>(Stream inStream, T inObj)
        {
            await JsonSerializer.SerializeAsync(inStream, inObj, options: _opts);
        }

        public async Task<T> DeserializeAsync<T>(Stream inStream)
        {
            var ret = await JsonSerializer.DeserializeAsync<T>(inStream);
            return ret;
        }

        public async Task<object> DeserializeAsync(Type type, Stream inStream)
        {
            if (inStream == null || type == null)
                throw new ArgumentNullException();

            return await JsonSerializer.DeserializeAsync(inStream, type);
        }
       
        private readonly static JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
        };
    }
}

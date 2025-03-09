
using Microsoft.AspNetCore.Mvc.Formatters;
using Protocol;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace Server.Formatter
{
    public class JsonDataSerializer : IDataSerializer
    {
        public string ContentType => MsgProtocol.JsonContentType;

        public byte[] Serialize<T>(T inObj)
        {
            var str = JsonSerializer.Serialize(inObj, options: Opts);
            var byteArr = Encoding.UTF8.GetBytes(str);
            return byteArr;
        }

        public async Task SerializeAsync<T>(Stream inStream, T inObj)
        {
            await JsonSerializer.SerializeAsync(inStream, inObj, options: Opts);
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

        public static string SerializeStr<T>(T inObj)
        {
            var str = JsonSerializer.Serialize(inObj, options: Opts);
            return str;
        }

        public static T DeserializeStr<T>(string json)
        {
            var ret = JsonSerializer.Deserialize<T>(json, options: Opts);
            return ret;
        }

        public readonly static JsonSerializerOptions Opts = new JsonSerializerOptions
        {
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() // .net 8.0 이상부터 설정 필요.
            // NOTE:  Ops에서 필드 전부 표시
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

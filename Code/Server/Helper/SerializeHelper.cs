using Microsoft.AspNetCore.Mvc.Rendering;
using NLog;
using System.Text.Json;

namespace WebStudyServer.Helper
{
    public static class SerializeHelper
    {
        public static string JsonSerialize(object obj) // TODO: 압축
        {
            var json = JsonSerializer.Serialize(obj);
            return json;
        }

        public static T JsonDeserialize<T>(string json) // TODO: 압축 해제
        {
            var obj = JsonSerializer.Deserialize<T>(json);
            return obj;
        }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    }
}

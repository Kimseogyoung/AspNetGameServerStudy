using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WebStudyServer.StartUp
{
    public static class StartUpExtension
    {
        public static List<string> GetValueStringList(this IConfiguration configuration, string key)
        {
            var strValue = configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(strValue))
            {
                return [];
            }

            var list = JsonSerializer.Deserialize<List<string>>(strValue)!;
            return list;
        }
    }
}

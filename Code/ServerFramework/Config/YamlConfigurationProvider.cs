using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebFramework.Config
{
    public class YamlConfigurationProvider : FileConfigurationProvider
    {
        // 참고 https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationProvider.cs
        public YamlConfigurationProvider(YamlConfigurationSource source) : base(source) { }

        /// <summary>
        /// Loads the JSON data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load(Stream stream)
        {

            var data = YamlConfigurationFileParser.Parse(stream);
            foreach (var (key, value) in data)
            {
                if (!Data.TryAdd(key, value))
                {
                    Data[key] = value;
                }
            }
        }
    }
}

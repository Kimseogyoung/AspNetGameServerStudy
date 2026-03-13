using System.Collections;
using System.Text.Json;
using SharpYaml.Serialization;

namespace WebFramework.Config
{
    public class YamlConfigurationFileParser
    {
        private readonly IDictionary<string, string?> _data = new Dictionary<string, string?>();
        private readonly Stack<string> _context = new Stack<string>();

        public static IDictionary<string, string?> Parse(Stream input)
        {
            var parser = new YamlConfigurationFileParser();
            return parser.ParseStream(input);
        }

        private IDictionary<string, string?> ParseStream(Stream input)
        {
            var serializer = new Serializer();
            var yamlObject = serializer.Deserialize<Dictionary<string, object>>(new StreamReader(input));

            if (yamlObject != null)
            {
                ParseYaml(yamlObject);
            }

            return _data;
        }

        private void ParseYaml(IDictionary<string, object> yamlDict)
        {
            foreach (var kvp in yamlDict)
            {
                EnterContext(kvp.Key);
                ParseValue(kvp.Value);
                ExitContext();
            }
        }

        private void ParseYaml(IDictionary<object, object> yamlDict)
        {
            foreach (var kvp in yamlDict)
            {
                EnterContext(kvp.Key.ToString());
                ParseValue(kvp.Value);
                ExitContext();
            }
        }

        private void ParseValue(object? value)
        {
            if (value is IDictionary<string, object> dict)
            {
                ParseYaml(dict);
            }
            else if (value is IDictionary<object, object> dict2)
            {
                ParseYaml(dict2);
            }
            else if (value is IList list)
            {
                var key = GetContextKey();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }

                _data[key] = JsonSerializer.Serialize(list);
            }
            else
            {
                var key = GetContextKey();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }

                _data[key] = value?.ToString();
            }
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
        }

        private void ExitContext()
        {
            _context.Pop();
        }

        private string GetContextKey()
        {
            return string.Join(":", _context.Reverse());
        }
    }

}

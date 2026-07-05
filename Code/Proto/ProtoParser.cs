using System;
using System.Collections.Generic;

namespace Proto
{
    public sealed class ProtoParser
    {
        public ProtoLoadResult<T> Parse<T>(string csvText) where T : ProtoBase, new()
        {
            var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2) return new ProtoLoadResult<T>();

            var rawNames = SplitLine(lines[0]);
            var rawTypes = SplitLine(lines[1]);

            var pkFields = new List<string>();
            var mkFields = new List<string>();
            var fieldNames = new List<string>();

            for (int i = 0; i < rawNames.Count; i++)
            {
                var name = rawNames[i];
                if (name.StartsWith("#")) continue;
                var typeStr = i < rawTypes.Count ? rawTypes[i] : "";

                fieldNames.Add(name);
                if (typeStr.EndsWith(":pk")) pkFields.Add(name);
                else if (typeStr.EndsWith(":mk")) mkFields.Add(name);
            }

            var items = new List<T>();
            for (int lineIdx = 2; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx];
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                var cells = SplitLine(line);
                if (cells.Count < fieldNames.Count) continue;

                var obj = new T();
                for (int j = 0; j < fieldNames.Count; j++)
                {
                    var cell = cells[j].Trim();
                    if (!string.IsNullOrEmpty(cell))
                        obj.SetField(fieldNames[j], cell);
                }
                items.Add(obj);
            }

            var result = new ProtoLoadResult<T>();
            result.Items = items;
            result.PkFields = pkFields;
            result.MkFields = mkFields;
            return result;
        }

        private static List<string> SplitLine(string line) => new List<string>(line.Split(','));
    }
}

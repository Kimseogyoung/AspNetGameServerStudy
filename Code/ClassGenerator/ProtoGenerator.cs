using Scriban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClassGenerator
{
    public static class ProtoGenerator
    {
        private static readonly Template s_template;

        static ProtoGenerator()
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Template", "ProtoTemplate.txt");
            s_template = Template.Parse(File.ReadAllText(templatePath));
        }

        public static void Run(string csvPath, string outputPath)
        {
            foreach (var csvFile in Directory.GetFiles(csvPath, "*.csv"))
            {
                var csvText = File.ReadAllText(csvFile);
                var className = Path.GetFileNameWithoutExtension(csvFile);

                if (!ParseFields(csvText, out var names, out var types, out var pkFields, out var mkFields)) continue;

                var fieldList = new List<Dictionary<string, object>>();
                for (int i = 0; i < names.Count; i++)
                {
                    var fieldName = names[i];
                    if (fieldList.Any(f => (string)f["Name"] == fieldName)) continue;
                    var typeStr = types[i];
                    var typeName = ToTypeName(typeStr);
                    fieldList.Add(new Dictionary<string, object>
                    {
                        ["Type"] = typeName,
                        ["Name"] = fieldName,
                        ["parse_expr"] = ToParseExpr(typeStr),
                        ["is_list"] = typeName.StartsWith("List<"),
                    });
                }

                var pkExpr = pkFields.Count == 1 ? pkFields[0]
                           : pkFields.Count > 1 ? $"({string.Join(", ", pkFields)})"
                           : "throw new System.NotSupportedException()";

                var mkExpr = mkFields.Count == 1 ? mkFields[0]
                           : mkFields.Count > 1 ? $"({string.Join(", ", mkFields)})"
                           : "";

                var scriptObject = new Dictionary<string, object>
                {
                    ["ClassName"] = $"{className}Proto",
                    ["Fields"] = fieldList,
                    ["pk_expr"] = pkExpr,
                    ["mk_expr"] = mkExpr,
                    ["has_mk"] = mkFields.Count > 0,
                };

                var result = s_template.Render(scriptObject);
                var outputFilePath = Path.GetFullPath(Path.Combine(outputPath, $"{className}Proto.generated.cs"));

                var dir = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(outputFilePath, result);
                Console.WriteLine($"Generated {className} → {outputFilePath}");
            }
        }

        private static bool ParseFields(string text, out List<string> fieldNames, out List<string> fieldTypes,
            out List<string> pkFields, out List<string> mkFields)
        {
            fieldNames = new List<string>();
            fieldTypes = new List<string>();
            pkFields = new List<string>();
            mkFields = new List<string>();

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2) return false;

            var names = lines[0].Split(',').ToList();
            var types = lines[1].Split(',').ToList();

            try
            {
                for (int i = 0; i < names.Count; i++)
                {
                    if (names[i].StartsWith("#")) continue;
                    var name = names[i];
                    fieldNames.Add(name);

                    var t = i < types.Count ? types[i] : "";
                    if (t.EndsWith(":pk")) { pkFields.Add(name); t = t.Substring(0, t.Length - 3); }
                    else if (t.EndsWith(":mk")) { mkFields.Add(name); t = t.Substring(0, t.Length - 3); }
                    fieldTypes.Add(t);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ParseFields error: {e.Message}");
                return false;
            }
        }

        private static string ToTypeName(string typeStr)
        {
            var colon = typeStr.IndexOf(':');
            if (colon == -1) return typeStr;
            var main = typeStr.Substring(0, colon);
            var sub = ToTypeName(typeStr.Substring(colon + 1));
            return main == "list" ? $"List<{sub}>" : sub;
        }

        private static string ToParseExpr(string typeStr)
        {
            var colon = typeStr.IndexOf(':');
            if (colon != -1) return ToParseExpr(typeStr.Substring(colon + 1)); // list → element expr

            switch (typeStr)
            {
                case "int": return "int.Parse(value)";
                case "float": return "float.Parse(value)";
                case "double": return "double.Parse(value)";
                case "bool": return "bool.Parse(value)";
                case "string": return "value";
                case "DateTime": return "DateTime.Parse(value)";
                default: return $"Enum.Parse<{typeStr}>(value)"; // enum
            }
        }
    }
}

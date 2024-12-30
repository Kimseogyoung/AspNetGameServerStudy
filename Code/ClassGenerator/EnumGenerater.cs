using DocumentFormat.OpenXml.Wordprocessing;
using Scriban;
using System.Text.RegularExpressions;

namespace ClassGenerator
{
    public class EnumGenerater
    {
        public static void Run(string argCsvPath, string argOutputPath)
        {
            var enumDefList = ParseCsv(argCsvPath);
            GenerateEnum(enumDefList, argOutputPath);
        }


        private static List<EnumDefinition> ParseCsv(string csvPath)
        {
            var files = Directory.GetFiles(csvPath, "*.csv", SearchOption.AllDirectories);
            var enumDefinitionList = new List<EnumDefinition>();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);

                // 첫 번째 줄은 헤더
                for (int i = 1; i < lines.Length; i++)
                {
                    var pattern = "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)";
                    var matches = Regex.Matches(lines[i], pattern);
                    var values = new List<string>();
                    foreach (Match match in matches)
                    {
                        var cell = match.Value;
                        if (cell.StartsWith("\"") && cell.EndsWith("\""))
                        {
                            cell = cell[1..^1].Replace("\"\"", "\""); // 이중 인용부호 제거 및 변환
                        }
                        values.Add(cell);
                    }

                    //var values = lines[i].Split(',');
                    values = values.Concat(Enumerable.Repeat("", c_maxColCnt - values.Count)).ToList();

                    if (string.IsNullOrEmpty(lines[i]) || values[0].StartsWith("#"))
                    {
                        // 주석 무시
                        continue;
                    }

                    var enumName = values[0];
                    var enumKey = values[1];
                    var enumValue = values[2];
                    var enumDesc = values[3];
                    enumDefinitionList.Add(new EnumDefinition
                    {
                        Name = enumName,
                        Key = enumKey,
                        Value = enumValue,
                        Description = enumDesc
                    });
                }
            }
            return enumDefinitionList;
        }

        private static void GenerateEnum(List<EnumDefinition> enumDefList, string argOutputPath)
        {
            var projectPath = GetProjPath();
            string templatePath = Path.Join(projectPath, "Template");
            string enumTemplatePath = Path.Join(templatePath, "EnumTemplate.txt");

            _template = File.ReadAllText(enumTemplatePath);
            var parsedTemplate = Template.Parse(_template);

            var groupedClassDict = new Dictionary<string, List<EnumDefinition>>();

            // 클래스 이름별로 필드 그룹화
            foreach (var definition in enumDefList)
            {
                if (!groupedClassDict.ContainsKey(definition.Name))
                    groupedClassDict[definition.Name] = new List<EnumDefinition>();


                groupedClassDict[definition.Name].Add(definition);
            }

            var enums = new List<dynamic>();
            foreach (var (className, defList) in groupedClassDict)
            {
                var fields = new List<dynamic>();
            
                for (var i = 0; i < defList.Count; i++)
                {
                    fields.Add(new Dictionary<string, object> {
                        { "Key",  defList[i].Key},
                        {"Value",  defList[i].Value},
                        {"Desc",  defList[i].Description},
                    });
                }

                enums.Add(new Dictionary<string, object> 
                {
                    { "Name",  className },
                    { "Fields",  fields }
                });            
            }

            var scriptObj = new Dictionary<string, object>()
            {
                { "Enums", enums } 
            };
            var result = parsedTemplate.Render(scriptObj);

            var outputFileName = $"Enum.generated.cs";
            var outputFilePath = Path.GetFullPath(Path.Join(argOutputPath, outputFileName));
            var directoryPath = Path.GetDirectoryName(outputFilePath);

            // 디렉토리가 없으면 생성
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(outputFilePath, result);
            Console.WriteLine($"Generate Enum {outputFilePath} done");
        }

        private static string GetProjPath()
        {
            var exeCfgDirNetPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var exeCfgDirPath = Path.GetDirectoryName(exeCfgDirNetPath);
            var binDirPath = Path.GetDirectoryName(exeCfgDirPath);
            var projectPath = Path.GetDirectoryName(binDirPath);
            return projectPath == null ? string.Empty : projectPath;
        }

        private static string _template = string.Empty;
        private const int c_maxColCnt = 4;
    }

    public class EnumDefinition
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}

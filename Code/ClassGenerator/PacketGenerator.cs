using Scriban;
using Scriban.Runtime;
using System.Security.Cryptography;

namespace ClassGenerator
{
    class PacketGenerator
    {
        public static void Run(string csvPath, string outputPath)
        {
            // CSV 파일 읽기
            var classDefList = ParseCsv(csvPath);
            GenerateClasses(classDefList, outputPath);
        }

        private static List<ClassDefinition> ParseCsv(string csvPath)
        {
            var files = Directory.GetFiles(csvPath);
            var classDefinitionList = new List<ClassDefinition>();

            foreach (var file in files)
            {
                if (!file.EndsWith(".csv"))
                {
                    continue;
                }

                var lines = File.ReadAllLines(file);

                // 첫 번째 줄은 헤더
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    values = values.Concat(Enumerable.Repeat("", c_maxColCnt - values.Length)).ToArray(); 

                    if (string.IsNullOrEmpty(lines[i]) || values[0].StartsWith("#"))
                    {
                        // 주석 무시
                        continue;
                    }

                    var classInfo = values[0];
                    if (!string.IsNullOrEmpty(classInfo))
                    {
                        classDefinitionList.Add(new ClassDefinition
                        {
                            ClassInfo = classInfo,
                            ClassName = values[1],
                            ProtocolName = values[6]
                        });
                    }
                    else
                    {
                        classDefinitionList.Add(new ClassDefinition
                        {
                            ClassName = values[1],
                            FieldName = values[2],
                            FieldType = values[3],
                            Idx = int.Parse(values[4]),
                            Description = values[5],
                        });
                    }
                }

            }
            return classDefinitionList;
        }

        private static void GenerateClasses(List<ClassDefinition> classDefinitions, string output)
        {
            var projectPath = GetProjPath();
            string templatePath = Path.Join(projectPath, "Template");
            string reqTemplatePath = Path.Join(templatePath, "ReqPacketTemplate.txt");
            string resTemplatePath = Path.Join(templatePath, "ResPacketTemplate.txt");
            string commonTemplatePath = Path.Join(templatePath, "CommonPacketTemplate.txt");
            _reqPakTemplate = File.ReadAllText(reqTemplatePath);
            _resPakTemplate = File.ReadAllText(resTemplatePath);
            _commonPakTemplate = File.ReadAllText(commonTemplatePath);

            var groupedClassDict = new Dictionary<string, List<ClassDefinition>>();

            // 클래스 이름별로 필드 그룹화
            foreach (var definition in classDefinitions)
            {
                if (!groupedClassDict.ContainsKey(definition.ClassName))
                    groupedClassDict[definition.ClassName] = new List<ClassDefinition>();


                groupedClassDict[definition.ClassName].Add(definition);
            }

            //var parsedTemplate = Template.Parse(template);
            foreach (var (className, defList) in groupedClassDict)
            {
                var template = "";
                var classAttribute = "[ProtoContract]";
                var firstDef = defList.First();
                defList.RemoveAt(0);

                var protocolName = "";
                switch (firstDef.ClassInfo)
                {
                    case "req":
                        template = _reqPakTemplate;
                        classAttribute = "[ProtoContract]";
                        protocolName = firstDef.ProtocolName;
                        defList.Insert(0, new ClassDefinition { FieldName = "Info", FieldType = "ReqInfoPacket", Idx = 1 , FieldValue = ""});
                        break;
                    case "res":
                        template = _resPakTemplate;
                        classAttribute = "[ProtoContract]";
                        defList.Insert(0, new ClassDefinition { FieldName = "Info", FieldType = "ResInfoPacket", Idx = 1 , FieldValue = "= new ResInfoPacket();" });
                        break;
                    default:
                        template = _commonPakTemplate;
                        classAttribute = "[ProtoContract]";
                        break;
                }

                var parsedTemplate = Template.Parse(template);
                var fields = new dynamic[defList.Count];
                for (var i =0; i< defList.Count; i++)
                {
                    var isLast = i == defList.Count - 1;
                    var attribute = $"[ProtoMember({defList[i].Idx})]";
                    var typeName = defList[i].FieldType;
                    var defValue = "= default;";
                    if (!_typeMap.ContainsKey(typeName))
                    {
                        defValue = $"= new {typeName}();";
                    }

                    fields[i] = new Dictionary<string, object> {
                        {"Type",  typeName },
                        {"Name",  defList[i].FieldName },
                        {"LowerName",  defList[i].FieldName.ToLower() },
                        {"Comma",  isLast? "" : "," },
                        { "Attribute",  attribute },
                         {"Value", defValue/* defList[i].FieldValue*/},
                    };

                }

                var classNameWithPak = $"{className}Packet";
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithPak},
                    { "ClassAttribute", classAttribute},
                    { "Fields", fields},
                    { "ProtocolName", protocolName}
                };

                var result = parsedTemplate.Render(scriptObject);

                var fileName = $"{classNameWithPak}.generated.cs";
                var outputFilePath = Path.GetFullPath(Path.Join(output, fileName));
                // 디렉토리 경로를 추출
                var directoryPath = Path.GetDirectoryName(outputFilePath);

                // 디렉토리가 없으면 생성
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(outputFilePath, result);
            }
        }

        private static string GetProjPath()
        {
            var exeCfgDirNetPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var exeCfgDirPath = Path.GetDirectoryName(exeCfgDirNetPath);
            var binDirPath = Path.GetDirectoryName(exeCfgDirPath);
            var projectPath = Path.GetDirectoryName(binDirPath);
            return projectPath == null? string.Empty : projectPath;
        }

        private const int c_maxColCnt = 7;
        private static string _reqPakTemplate = string.Empty;
        private static string _resPakTemplate = string.Empty;
        private static string _commonPakTemplate = string.Empty;

        private static Dictionary<string, Type>  _typeMap = new Dictionary<string, Type>
        {
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "char", typeof(char) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "string", typeof(string) }
        };
    }

    public class ClassDefinition
    {
        public string ClassInfo { get; set; }
        public string ClassName { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldValue { get; set; }
        public int Idx { get; set; }
        public string Description { get; set; }
        public string ProtocolName { get; set; }
    }
}

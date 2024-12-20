using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using System.Security.Cryptography;

namespace ClassGenerator
{
    class ModelGenerator
    {
        public static void Run(string argCsvPath, string argMdlOutputPath, string argPakOutputPath, string argLiquibaseOutputPath)
        {
            // CSV 파일 읽기
            var classDefList = ParseCsv(argCsvPath);
            GenerateClasses(classDefList, argMdlOutputPath, argPakOutputPath, argLiquibaseOutputPath);
        }

        public static List<ModelDefinition> ParseCsv(string csvPath)
        {
            var rootDirName = Path.GetFileName(csvPath);
            var files = Directory.GetFiles(csvPath, "*.csv", SearchOption.AllDirectories);
            var classDefinitionList = new List<ModelDefinition>();

            foreach (var file in files)
            {
                var dirName = Path.GetFileName(Path.GetDirectoryName(file));
                var lines = File.ReadAllLines(file);
                var className = Path.GetFileName(file).Replace(".csv", "");

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

                    var folderName = dirName != rootDirName ? dirName : "";
                    var fieldName = values[0];
                    var typeArr = values[1].Split(":");
                    var fieldType = typeArr[0];
                    var fieldDesc = values[3];
                    var protocolType = values[4];
                    var fieldValue = string.IsNullOrEmpty(values[2]) ? "default" : values[2];
                    var keyList = values[5].Split(",").ToList();
                    
                    var fieldCodeType = "";
                    var fieldSQLType = "";
                    switch (fieldType)
                    {
                        case "BIGINT UNSIGNED":
                            fieldSQLType = fieldType;
                            fieldCodeType = "ulong";
                            break;
                        case "BIGINT":
                            fieldSQLType = fieldType;
                            fieldCodeType = "long";
                            break;
                        case "INT":
                            fieldSQLType = fieldType;
                            fieldCodeType = "int";
                            break;
                        case "DATETIME":
                            fieldSQLType = fieldType;
                            fieldCodeType = "DateTime";
                            break;
                        case "DOUBLE":
                            fieldSQLType = fieldType;
                            fieldCodeType = "double";
                            break;
                        case "ENUM":
                            fieldSQLType = "INT";
                            fieldCodeType = typeArr[1];
                            break;
                        case "LIST":
                            if (protocolType != "Packet")
                            {
                                throw new Exception($"LIST_CAN_NOT_SET_MODEL:{fieldName}");
                            }
                            fieldSQLType = ""; 
                            fieldCodeType = $"List<{typeArr[1]}>";
                            break;
                        default:
                            if (fieldType.StartsWith("VARCHAR"))
                            {
                                fieldSQLType = fieldType;
                                fieldCodeType = "string";
                                break;
                            }
                            throw new Exception($"NO_HANDLING_FIELD_TYPE:{fieldType}");
                    }


                    classDefinitionList.Add(new ModelDefinition
                    {
                        FolderName = folderName,
                        ClassName = className,
                        FieldName = fieldName,
                        FieldCodeType = fieldCodeType,
                        FieldSQLType = fieldSQLType,
                        FieldValue = fieldValue,
                        Description = fieldDesc,
                        ProtocolType = protocolType,
                        KeyList = keyList
                    });
                }

            }
            return classDefinitionList;
        }

        public static void GenerateClasses(List<ModelDefinition> classDefinitions, string mdlOutputPath, string pakOutputPath, string liquibaseOutputPath)
        {
            var projectPath = GetProjPath();
            string templatePath = Path.Join(projectPath, "Template");
            string pakTemplatePath = Path.Join(templatePath, "PacketTemplate.txt");
            string mdlTemplatePath = Path.Join(templatePath, "ModelTemplate.txt");

            _pakTemplate = File.ReadAllText(pakTemplatePath);
            _mdlTemplate = File.ReadAllText(mdlTemplatePath);

            var groupedClassDict = new Dictionary<string, List<ModelDefinition>>();

            // 클래스 이름별로 필드 그룹화
            foreach (var definition in classDefinitions)
            {
                if (!groupedClassDict.ContainsKey(definition.ClassName))
                    groupedClassDict[definition.ClassName] = new List<ModelDefinition>();


                groupedClassDict[definition.ClassName].Add(definition);
            }

            GenerateModel(groupedClassDict, mdlOutputPath);
            GeneratePacket(groupedClassDict, pakOutputPath);

            GenerateLiquibaseChangeLog(groupedClassDict, liquibaseOutputPath);
        }

        public static void GenerateModel(Dictionary<string, List<ModelDefinition>> modelDefListDict, string mdlOutputPath)
        {
            foreach (var (className, defList) in modelDefListDict)
            {
                var parsedTemplate = Template.Parse(_mdlTemplate);
                var fieldList = new List<dynamic>();
                for (var i = 0; i < defList.Count; i++)
                {
                    if (defList[i].ProtocolType == "Packet")
                    {
                        continue;
                    }

                    var attribute = $"[ProtoMember({i})]";
                    var field = new Dictionary<string, object> {
                        {"Type",  defList[i].FieldCodeType },
                        {"Name",  defList[i].FieldName },
                        {"Attribute",  "" },
                        {"Value",  defList[i].FieldValue},
                        {"Desc", defList[i].Description }
                    };
                    fieldList.Add(field);
                }

                var classNameWithMdl = $"{className}Model";
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithMdl},
                    { "ClassAttribute", ""},
                    { "Fields", fieldList},
                };

                var result = parsedTemplate.Render(scriptObject);
                var fileName = $"{classNameWithMdl}.generated.cs";
                var folderName = defList[0].FolderName;
                var outputFilePath = Path.GetFullPath(Path.Join(mdlOutputPath, folderName, fileName));
                var directoryPath = Path.GetDirectoryName(outputFilePath);

                // 디렉토리가 없으면 생성
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(outputFilePath, result);
            }
        }

        public static void GenerateLiquibaseChangeLog(Dictionary<string, List<ModelDefinition>> modelDefListDict, string mdlOutputPath)
        {
            foreach (var (className, defList) in modelDefListDict)
            {
/*                var parsedTemplate = Template.Parse(_mdlTemplate);
                var fields = new dynamic[defList.Count];
                for (var i = 0; i < defList.Count; i++)
                {
                    fields[i] = new Dictionary<string, object> {
                        {"Type",  defList[i].FieldSQLType },
                        {"Name",  defList[i].FieldName },
                        {"Attribute",  "" },
                        {"Value",  defList[i].FieldValue},
                        {"Desc", defList[i].Description }
                    };

                }

                var classNameWithMdl = $"{className}Model";
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithMdl},
                    { "ClassAttribute", ""},
                    { "Fields", fields},
                };

                var result = parsedTemplate.Render(scriptObject);
                var fileName = $"{classNameWithMdl}.generated.cs";
                var outputFilePath = Path.GetFullPath(Path.Join(mdlOutputPath, fileName));
                var directoryPath = Path.GetDirectoryName(outputFilePath);

                // 디렉토리가 없으면 생성
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(outputFilePath, result);*/
            }
        }

        public static void GeneratePacket(Dictionary<string, List<ModelDefinition>> modelDefListDict, string pakOutputPath)
        {
            foreach (var (className, defList) in modelDefListDict)
            {
                var parsedTemplate = Template.Parse(_pakTemplate);
                var fieldList = new List<dynamic>();
                for (var i = 0; i < defList.Count; i++)
                {
                    if (defList[i].ProtocolType == "Model")
                    {
                        continue;
                    }

                    var attribute = $"[ProtoMember({i})]";
                    var field = new Dictionary<string, object> {
                        {"Type",  defList[i].FieldCodeType },
                        {"Name",  defList[i].FieldName },
                        {"Attribute",  attribute },
                        {"Value",  defList[i].FieldValue},
                        {"Desc", defList[i].Description }
                    };
                    fieldList.Add(field);
                }

                var classNameWithPak = $"{className}Packet";
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithPak},
                    { "ClassAttribute", "[ProtoContract]"},
                    { "Fields", fieldList},
                };

                var result = parsedTemplate.Render(scriptObject);
                var fileName = $"{classNameWithPak}.generated.cs";
                var folderName = defList[0].FolderName;
                var outputFilePath = Path.GetFullPath(Path.Join(pakOutputPath, folderName, fileName));
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

        private const int c_maxColCnt = 6;
        private static string _pakTemplate = string.Empty;
        private static string _mdlTemplate = string.Empty;
    }

    public class ModelDefinition
    {
        public string FolderName { get; set; }
        public string ClassName { get; set; }
        public string FieldName { get; set; }
        public string FieldCodeType { get; set; }
        public string FieldSQLType { get; set; }
        public string FieldValue { get; set; }
        public string ProtocolType { get; set; }
        public string Description { get; set; }
        public List<string> KeyList { get; set; }
    }
}

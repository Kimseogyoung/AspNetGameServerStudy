using Scriban;

namespace ClassGenerator
{
    public class ProtoGenerater
    {
        public static void Run(string argCsvPath, string argOutputPath)
        {
            GenerateProto(argCsvPath, argOutputPath);
        }

        private static void GenerateProto(string argCsvPath, string argOutputPath)
        {
            var projectPath = GetProjPath();
            var templatePath = Path.Join(projectPath, "Template");
            var prtTemplatePath = Path.Join(templatePath, "ProtoTemplate.txt");
            _template = File.ReadAllText(prtTemplatePath);

            var csvFilePaths = Directory.GetFiles(argCsvPath, "*.csv");


            for (int i = 0; i < csvFilePaths.Length; i++)
            {
                var csvFile = csvFilePaths[i];
                var fileTxt = File.ReadAllText(csvFile);
                var className = Path.GetFileName(csvFile).Replace(".csv", "");
                if (!LoadProtoCsvField(out string pkName, out List<string> names, out List<string> types, fileTxt))
                {
                    continue;
                }

                var parsedTemplate = Template.Parse(_template);
                var fieldList = new List<dynamic>();

                for (int j = 0; j < names.Count; j++)
                {
                    var fieldName = names[j];
                    var fieldType = types[j];

                    int idx = fieldType.IndexOf(":");
                    if (idx != -1)
                    {
                        fieldType = fieldType.Substring(idx + 1, fieldType.Length - (idx + 1));
                    }

                    var prtIdx = j + 2;
                    var attribute = $"[ProtoMember({prtIdx})]";
                    var field = new Dictionary<string, object> {
                        {"Type",  fieldType },
                        {"Name",  fieldName},
                        {"Attribute", attribute },
                    };
                    fieldList.Add(field);
                }

                var classNameWithPrt = $"{className}Proto";
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithPrt},
                    { "ClassAttribute", "[ProtoContract]"},
                    { "Fields", fieldList },
                };

                var result = parsedTemplate.Render(scriptObject);

                var outputFileName = $"{className}Proto.generated.cs";
                var folderName = "";
                var outputFilePath = Path.GetFullPath(Path.Join(argOutputPath, folderName, outputFileName));
                var directoryPath = Path.GetDirectoryName(outputFilePath);

                // 디렉토리가 없으면 생성
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(outputFilePath, result);
                Console.WriteLine($"Generate {className} from {csvFilePaths[i]} : {outputFilePath} done");

            }
        }

        private static bool LoadProtoCsvField(out string pkName, out List<string> fieldNames, out List<string> fieldTypes, string text)
        {
            string[] lines = text.Split("\r\n");
            pkName = string.Empty;
            fieldNames = new List<string>();
            fieldTypes = new List<string>();

            List<string> names = lines[0].Split(",").ToList<string>();
            List<string> types = lines[1].Split(",").ToList<string>();

            try
            {
                for (int i = 0; i < names.Count; i++)
                {

                    if (names[i].StartsWith("#")) continue;
                    fieldNames.Add(names[i]);

                    if (types[i].EndsWith(":pk"))
                    {
                        types[i] = types[i].Replace(":pk", "");
                        pkName = names[i];
                    }


                    fieldTypes.Add(types[i]);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
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
        private Dictionary<string, Type> _typeMappingDict = new Dictionary<string, Type>();
    }
}

using Scriban;
using System.Text.RegularExpressions;
using System.Text.Json;
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

                    var folderName = dirName != rootDirName ? dirName : "";
                    var fieldName = values[0];
                    var typeArr = values[1].Split(":");
                    var fieldType = typeArr[0];
                    var fieldDesc = values[3];
                    var protocolType = values[4];
                    var fieldValue = string.IsNullOrEmpty(values[2]) ? "default" : values[2];
                    var keyList = values[5].Split(",").Select(x=>x.Trim()).ToList();
                    
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

            foreach (var (tableName, defList) in groupedClassDict)
            {
                defList.Add(new ModelDefinition
                {
                    ClassName = tableName,
                    FieldCodeType = "DateTime",
                    FieldSQLType = "DATETIME",
                    FieldName = "UpdateTime",
                    FieldValue = "default",
                    ProtocolType = "Model"
                });

                defList.Add(new ModelDefinition
                {
                    ClassName = tableName,
                    FieldCodeType = "DateTime",
                    FieldSQLType = "DATETIME",
                    FieldName = "CreateTime",
                    FieldValue = "default",
                    ProtocolType = "Model"
                });
            }


            GenerateLiquibaseChangeLog(groupedClassDict, liquibaseOutputPath);
        }

        public static void GenerateModel(Dictionary<string, List<ModelDefinition>> modelDefListDict, string mdlOutputPath)
        {
            foreach (var (className, defList) in modelDefListDict)
            {
                var parsedTemplate = Template.Parse(_mdlTemplate);
                var fieldList = new List<dynamic>();

                foreach (var def in defList.Where(x => x.ProtocolType != "Packet"))
                {
                    var field = new Dictionary<string, object> {
                        {"Type",  def.FieldCodeType },
                        {"Name",  def.FieldName },
                        {"Attribute",  "" },
                        {"Value",  def.FieldValue},
                        {"Desc", def.Description }
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
            var folderTableNameDict = new Dictionary<string, List<string>>();
            foreach(var (key, defList) in modelDefListDict)
            {
                var folderName = defList[0].FolderName;
                if (folderTableNameDict.ContainsKey(folderName))
                {
                    folderTableNameDict[folderName].Add(key);
                }
                else
                {
                    folderTableNameDict[folderName] = new List<string>() { key };
                }
            }

            foreach(var (folderName, tableNameList) in folderTableNameDict)
            {
                var databaseChangeLog = new DatabaseChangeLogData();
                var databaseChange1 = new DatabaseChangeLog
                {
                    PreConditions = new List<PreConditions>() {  new PreConditions
                {
                    RunningAs = new RunningAs{ Username = "root" }
                }}
                };
                databaseChangeLog.DatabaseChangeLog.Add(databaseChange1);

                foreach (var (className, defList) in 
                    modelDefListDict.Where(x=> tableNameList.Contains(x.Key))
                    .OrderBy(x => x.Key != "Player" && x.Key != "Account").ThenBy(x => x.Key))
                {
                    var mdlDefList = defList.Where(x => x.ProtocolType != "Packet").ToList();
                    var databaseChange = new DatabaseChangeLog
                    {
                        ChangeSet = new ChangeSet
                        {
                            Id = className,
                            Author = "seogyoung",
                            Changes = new List<Change>()
                        }
                    };
                    databaseChangeLog.DatabaseChangeLog.Add(databaseChange);

                    // 테이블 생성
                    var createTableChange = new Change
                    {
                        CreateTable = new CreateTable
                        {
                            TableName = className,
                        }
                    };
                    databaseChange.ChangeSet.Changes.Add(createTableChange);

                    foreach (var def in mdlDefList)
                    {
                        createTableChange.CreateTable.Columns.Add(new Columns
                        {
                            Column = new Column
                            {
                                Name = def.FieldName,
                                Type = def.FieldSQLType,
                                Constraints = new ColumnConstraints { Nullable = false, PrimaryKey = false }
                            }
                        });
                    }

                    // 복합 커맨드
                    var pkDefs = mdlDefList.Where(x => x.KeyList.Contains("pk"));
                    if (pkDefs.Any())
                    {
                        var columnNames = string.Join(", ", pkDefs.Select(x => x.FieldName));
                        var pkChange = new Change
                        {
                            AddPrimaryKey = new AddPrimaryKey
                            {
                                TableName = className,
                                ColumnNames = columnNames
                            }
                        };
                        databaseChange.ChangeSet.Changes.Add(pkChange);
                    }

                    var indexDefs = mdlDefList.Where(x => x.KeyList.Contains("c_index"));
                    if (indexDefs.Any())
                    {
                        var createIndex = new CreateIndex
                        {
                            IndexName = $"{className}_Key_Index",
                            TableName = className,
                        };

                        foreach (var def in indexDefs)
                        {
                            createIndex.Columns.Add(new Columns { Column = new Column { Name = def.FieldName } });
                        }

                        var indexChange = new Change
                        {
                            CreateIndex = createIndex
                        };
                        databaseChange.ChangeSet.Changes.Add(indexChange);
                    }

                    foreach (var def in mdlDefList)
                    {
                        // 단일 커맨드
                        foreach (var key in def.KeyList)
                        {
                            var keyStr = key.Trim();
                            switch (keyStr)
                            {
                                case "autogenerated":
                                    var autoGen = new AddAutoIncrement
                                    {
                                        TableName = className,
                                        ColumnDataType = def.FieldSQLType,
                                        ColumnName = def.FieldName,
                                    };
                                    var autoGenChange = new Change
                                    {
                                        AddAutoIncrement = autoGen
                                    };
                                    databaseChange.ChangeSet.Changes.Add(autoGenChange);
                                    break;
                                case "fk":

                                    var referTableName = folderName == "User" ? "Player" : "Account";
                                    var addFk = new AddForeignKeyConstraint
                                    {
                                        BaseTableName = className,
                                        BaseColumnNames = def.FieldName,
                                        ReferencedTableName = referTableName,
                                        ReferencedColumnNames = "Id",
                                        ConstraintName = $"FK_{className}_{referTableName}"
                                    };
                                    var addFkChange = new Change
                                    {
                                        AddForeignKeyConstraint = addFk
                                    };
                                    databaseChange.ChangeSet.Changes.Add(addFkChange);
                                    break;
                                case "index":

                                    var createIndex = new CreateIndex
                                    {
                                        IndexName = $"{className}_{def.FieldName}_Index",
                                        TableName = className,
                                        Columns = new List<Columns>() { new Columns { Column = new Column { Name = def.FieldName } } }
                                    };
                                    var createIndexChange = new Change
                                    {
                                        CreateIndex = createIndex
                                    };
                                    databaseChange.ChangeSet.Changes.Add(createIndexChange);
                                    break;
                            }
                        }
                    }
                }

                var json = JsonSerializer.Serialize(databaseChangeLog, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // CamelCase naming convention
                    WriteIndented = true, // Enable pretty printing
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // Ignore null values

                });

                var fileName = $"CreateLog_{folderName}.json";
                var filePath = Path.Join(mdlOutputPath, fileName);
                var directoryPath = Path.GetDirectoryName(mdlOutputPath);

                // 디렉토리가 없으면 생성
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(filePath, json);

            }      
        }

        public static void GeneratePacket(Dictionary<string, List<ModelDefinition>> modelDefListDict, string pakOutputPath)
        {
            foreach (var (className, defList) in modelDefListDict)
            {
                var parsedTemplate = Template.Parse(_pakTemplate);
                var fieldList = new List<dynamic>();
                var index = 1;
                foreach (var def in defList.Where(x=>x.ProtocolType != "Model"))
                {
                    var attribute = $"[ProtoMember({index})]";
                    var field = new Dictionary<string, object> {
                        {"Type",  def.FieldCodeType },
                        {"Name",  def.FieldName },
                        {"Attribute",  attribute },
                        {"Value",  def.FieldValue},
                        {"Desc", def.Description }
                    };
                    fieldList.Add(field);
                    index++;
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
        public List<string> KeyList { get; set; } = new();
    }
}

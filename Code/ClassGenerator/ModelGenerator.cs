using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Scriban;
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

                    if (string.IsNullOrEmpty(lines[i]) || values.Count == 0 || values[0].StartsWith("#"))
                    {
                        // 주석 무시
                        continue;
                    }

                    // 칸이 넘치면 아래 Repeat 이 음수 count 로 죽는다. 어느 파일 몇 행인지 알려준다.
                    if (values.Count > c_maxColCnt)
                    {
                        throw new Exception(
                            $"TOO_MANY_COLUMNS:{Path.GetFileName(file)}:line {i + 1}:{values.Count}/{c_maxColCnt}"
                            + " - 따옴표 없는 콤마가 셀에 있는지 확인");
                    }

                    values = values.Concat(Enumerable.Repeat("", c_maxColCnt - values.Count)).ToList();

                    var folderName = dirName != rootDirName ? dirName : "";
                    var fieldName = values[0];
                    var typeArr = values[1].Split(":");
                    var fieldType = typeArr[0];
                    var fieldDesc = values[3];
                    var protocolType = values[4];
                    var fieldValue = string.IsNullOrEmpty(values[2]) ? "default" : values[2];
                    var keyList = values[5].Split(",").Select(x => x.Trim()).ToList();

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
                            if (fieldType.StartsWith("VARCHAR") || fieldType.Contains("TEXT"))
                            {
                                fieldSQLType = fieldType;
                                fieldCodeType = "string";
                                break;
                            }

                            if (protocolType == "Packet")
                            {
                                fieldSQLType = fieldType;
                                fieldCodeType = fieldType;
                                break;
                            }

                            throw new Exception($"NO_HANDLING_FIELD_TYPE:{fieldType}");
                    }


                    // .net standard 2.1 에서 new()키워드를 못쓰는 이슈때문에 임시처리
                    fieldValue = fieldValue == "new()" ? $"new {fieldCodeType}()" : fieldValue;

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
                {
                    groupedClassDict[definition.ClassName] = new List<ModelDefinition>();
                }

                groupedClassDict[definition.ClassName].Add(definition);
            }

            GeneratePacket(groupedClassDict, pakOutputPath);
            GenerateModel(groupedClassDict, mdlOutputPath);

            foreach (var (tableName, defList) in groupedClassDict)
            {
                var mdlCnt = GetModelFieldCnt(defList);
                if (mdlCnt == 0)
                {
                    continue;
                }

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
                var mdlCnt = GetModelFieldCnt(defList);
                if (mdlCnt == 0)
                {
                    continue;
                }

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
                var keys = ResolveModelKeys(className, defList);
                var scriptObject = new Dictionary<string, object>
                {
                    { "ClassName",  classNameWithMdl},
                    { "ClassAttribute", BuildEntityAttribute(keys)},
                    { "BaseTypes", BuildBaseTypes(keys)},
                    { "Members", BuildMembers(classNameWithMdl, keys)},
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

        // 모델의 키 규칙을 한 번 푼다. [Entity] / 상속 목록 / 생성 멤버가 모두 이 결과를 쓴다.
        //
        // 규칙이 애매한 경우는 추측하지 않고 생성을 실패시킨다. 여기서 조용히 틀린
        // 컬럼을 고르면 그 값이 PK WHERE 절과 소유자 필터로 그대로 흘러가고,
        // 증상은 "0행 매치"나 "남의 데이터 조회"처럼 예외 없이 나타난다.
        private static ModelKeys ResolveModelKeys(string className, List<ModelDefinition> defList)
        {
            // Packet 전용 필드는 테이블에 없으므로 키 판정에서 제외한다.
            // (GenerateLiquibaseChangeLog 도 같은 기준으로 컬럼을 고른다)
            var mdlDefList = defList.Where(x => x.ProtocolType != "Packet").ToList();

            var pkList = mdlDefList.Where(x => x.KeyList.Contains("pk")).Select(x => x.FieldName).ToList();
            if (pkList.Count == 0)
            {
                throw new Exception($"MISSING_PK:{className}");
            }

            // 소유자 개념이 있는 것은 User 계열뿐이다. Auth/Center 에는 그런 축이 없다.
            var folderName = defList[0].FolderName;
            if (folderName != "User")
            {
                return new ModelKeys(pkList, null);
            }

            // Player 는 스코프 루트라 fk 가 없는 것이 정상이고, 자기 PK 가 곧 스코프 키다.
            // 이름으로 특수 처리한다 - "fk 가 없으면 PK 를 쓴다"로 일반화하면 fk 를
            // 빠뜨린 User 모델이 자기 PK 를 스코프 키로 갖게 되어 소유자 필터가 사라진다.
            string scopeKey;
            if (className == "Player")
            {
                if (pkList.Count != 1)
                {
                    throw new Exception($"SCOPE_ROOT_COMPOSITE_PK:{className}");
                }

                scopeKey = pkList[0];
            }
            else
            {
                var fkList = mdlDefList.Where(x => x.KeyList.Contains("fk")).Select(x => x.FieldName).ToList();
                if (fkList.Count == 0)
                {
                    throw new Exception($"MISSING_SCOPE_KEY:{className}");
                }
                if (fkList.Count > 1)
                {
                    throw new Exception($"AMBIGUOUS_SCOPE_KEY:{className}");
                }

                scopeKey = fkList[0];
            }

            // IScopedModel 의 접근자가 ulong 하나로 고정돼 있다. 다른 타입이면 안 맞으므로 실패시킨다.
            var scopeKeyType = mdlDefList.First(x => x.FieldName == scopeKey).FieldCodeType;
            if (scopeKeyType != "ulong")
            {
                throw new Exception($"NOT_ULONG_SCOPE_KEY:{className}.{scopeKey}:{scopeKeyType}");
            }

            return new ModelKeys(pkList, scopeKey);
        }

        private static string BuildEntityAttribute(ModelKeys keys)
        {
            var pkArg = string.Join(", ", keys.PkList.Select(x => $"\"{x}\""));
            return keys.ScopeKey == null
                ? $"[Entity(Pk = [{pkArg}])]"
                : $"[Entity(Pk = [{pkArg}], ScopeKey = \"{keys.ScopeKey}\")]";
        }

        private static string BuildBaseTypes(ModelKeys keys)
        {
            return keys.ScopeKey == null ? "ModelBase" : "ModelBase, IScopedModel";
        }

        // PkEquals 는 캐시 리스트의 항목 교체에, GetScopeKey/SetScopeKey 는 소유자 확인과
        // 생성 시 소유자 채움에 쓴다. 찍어낼 수 있는 정보를 문자열로 흘린 뒤
        // 리플렉션으로 되사오지 않으려고 여기서 코드로 만든다.
        private static string BuildMembers(string classNameWithMdl, ModelKeys keys)
        {
            var sb = new StringBuilder();

            sb.AppendLine("\t\tpublic override bool PkEquals(ModelBase other)");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\treturn other is {classNameWithMdl} otherModel");
            sb.AppendLine(string.Join(Environment.NewLine, keys.PkList.Select(x => $"\t\t\t\t&& {x} == otherModel.{x}")) + ";");
            sb.AppendLine("\t\t}");

            if (keys.ScopeKey != null)
            {
                sb.AppendLine();
                sb.AppendLine($"\t\tpublic ulong GetScopeKey() => {keys.ScopeKey};");
                sb.AppendLine($"\t\tpublic void SetScopeKey(ulong value) => {keys.ScopeKey} = value;");
            }

            return sb.ToString().TrimEnd();
        }

        // ScopeKey: 소유자 컬럼명. Auth/Center 계열은 null.
        private record ModelKeys(List<string> PkList, string? ScopeKey);

        public static void GenerateLiquibaseChangeLog(Dictionary<string, List<ModelDefinition>> modelDefListDict, string mdlOutputPath)
        {
            var folderTableNameDict = new Dictionary<string, List<string>>();
            foreach (var (key, defList) in modelDefListDict)
            {
                var mdlCnt = GetModelFieldCnt(defList);
                if (mdlCnt == 0)
                {
                    continue;
                }

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

            foreach (var (folderName, tableNameList) in folderTableNameDict)
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
                    modelDefListDict.Where(x => tableNameList.Contains(x.Key))
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
                foreach (var def in defList.Where(x => x.ProtocolType != "Model"))
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
            return projectPath == null ? string.Empty : projectPath;
        }

        private static int GetModelFieldCnt(List<ModelDefinition> defList)
        {
            var mdlCnt = defList.Count(x => string.IsNullOrEmpty(x.ProtocolType) || x.ProtocolType == "Model");
            return mdlCnt;
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

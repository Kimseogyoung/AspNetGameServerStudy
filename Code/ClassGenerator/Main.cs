using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassGenerator
{
    public class ClassGenerater
    {
        public static void Main(string[] args)
        {
            // 기본 값 설정
            string type = "";
            string argXlsxPath = "";
            string argCsvPath = "";
            string argOutputPath = "";
            string argMdlOutputPath = "";
            string argPakOutputPath = "";
            string argLiquibaseOutputPath = "";

            var projectPath = GetProjPath();

            // 명령줄 인수 처리
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--type" && i + 1 < args.Length)
                {
                    type = args[i + 1];
                }
                else if (args[i] == "--xlsxPath" && i + 1 < args.Length)
                {
                    argXlsxPath = args[i + 1];
                }
                else if (args[i] == "--csvPath" && i + 1 < args.Length)
                {
                    argCsvPath = args[i + 1];
                }
                else if (args[i] == "--outputPath" && i + 1 < args.Length)
                {
                    argOutputPath = args[i + 1];
                }
                else if (args[i] == "--mdlOutputPath" && i + 1 < args.Length)
                {
                    argMdlOutputPath = args[i + 1];
                }
                else if (args[i] == "--pakOutputPath" && i + 1 < args.Length)
                {
                    argPakOutputPath = args[i + 1];
                }
                else if (args[i] == "--liquibaseOutputPath" && i + 1 < args.Length)
                {
                    argLiquibaseOutputPath = args[i + 1];
                }
            }

            string xlsxPath = Path.Join(projectPath, argXlsxPath);
            string csvPath = Path.Join(projectPath, argCsvPath);

            if (string.IsNullOrEmpty(xlsxPath))
            {
                throw new Exception("NULL_EMPTY_XLSX");
            }

            if (string.IsNullOrEmpty(csvPath))
            {
                throw new Exception("NULL_EMPTY_CSV");
            }

            ConvertXlsxToCsv(xlsxPath, csvPath);

            switch (type)
            {
                case "Packet":
                    {
                        string outputPath = Path.Join(projectPath, argOutputPath);
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            throw new Exception("NULL_EMPTY__OUTPUT");
                        }

                        // CSV 파일 읽기
                        PacketGenerator.Run(csvPath, outputPath);
                        break;
                    }
                case "Model":
                    {
                        string mdlOutputPath = Path.Join(projectPath, argMdlOutputPath);
                        string pakOutputPath = Path.Join(projectPath, argPakOutputPath);
                        string liquibaseOutputPath = Path.Join(projectPath, argLiquibaseOutputPath);
                        if (string.IsNullOrEmpty(mdlOutputPath) || string.IsNullOrEmpty(pakOutputPath) || string.IsNullOrEmpty(liquibaseOutputPath))
                        {
                            throw new Exception("NULL_EMPTY__OUTPUT");
                        }

                        // CSV 파일 읽기
                        ModelGenerator.Run(csvPath, mdlOutputPath, pakOutputPath, liquibaseOutputPath);
                        break;
                    }
                case "Proto":
                    {
                        string outputPath = Path.Join(projectPath, argOutputPath);
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            throw new Exception("NULL_EMPTY__OUTPUT");
                        }

                        // CSV 파일 읽기
                        ProtoGenerater.Run(csvPath, outputPath);
                        break;
                    }
                case "Enum":
                    {
                        string outputPath = Path.Join(projectPath, argOutputPath);
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            throw new Exception("NULL_EMPTY__OUTPUT");
                        }

                        // CSV 파일 읽기
                        EnumGenerater.Run(csvPath, outputPath);
                        break;
                    }
                default:
                    throw new Exception($"NO_HANDLING_GENERATER_TYPE:{type}");
            }
        }

        private static List<ClassDefinition> ConvertXlsxToCsv(string xlsxPath, string csvPath)
        {
            var rootDirName = Path.GetFileName(csvPath);
            var files = Directory.GetFiles(xlsxPath, "*.xlsx", SearchOption.AllDirectories);
            var classDefinitionList = new List<ClassDefinition>();

            // 디렉토리가 없으면 생성
            if (!string.IsNullOrEmpty(csvPath) && !Directory.Exists(csvPath))
            {
                Directory.CreateDirectory(csvPath);
            }

            foreach (var xlsxFile in files)
            {
                var dirName = Path.GetFileName(Path.GetDirectoryName(xlsxFile));
                var csvDirName = rootDirName == dirName ? "" : dirName;
                var csvFileName = Path.GetFileName(xlsxFile).Replace("xlsx", "csv");

                if (csvFileName.StartsWith("_"))
                {
                    continue;
                }

                var filePath = Path.Join(csvPath, csvDirName, csvFileName);
                ExcelToCSVConverter.ConvertExcelToCSV(xlsxFile, filePath);
            }
            return classDefinitionList;
        }

        private static string GetProjPath()
        {
            var exeCfgDirNetPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var exeCfgDirPath = Path.GetDirectoryName(exeCfgDirNetPath);
            var binDirPath = Path.GetDirectoryName(exeCfgDirPath);
            var projectPath = Path.GetDirectoryName(binDirPath);
            return projectPath == null ? string.Empty : projectPath;
        }
    }
}

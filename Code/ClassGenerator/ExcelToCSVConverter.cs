using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Text;

namespace ClassGenerator
{
    class ExcelToCSVConverter
    {   
        public static void ConvertExcelToCSV(string excelFilePath, string csvFilePath)
        {
            // XLSX 파일 열기
            using (var workbook = new XLWorkbook(excelFilePath))
            {
                // CSV로 쓸 StringBuilder 생성
                var csvContent = new StringBuilder();

                // 워크시트 순차 처리
                int worksheetIndex = 0;
                foreach (var worksheet in workbook.Worksheets)
                {
                    // 두 번째 워크시트부터 첫 번째 행 삭제
                    if (worksheetIndex >= 1)
                    {
                        worksheet.Rows(1, 1).Delete(); // 첫 번째 행(헤더) 삭제
                    }

                    bool isFirstRow = true;
                    var headerColCnt = 0;
                    // 각 행(row)을 순차적으로 처리
                    foreach (var row in worksheet.Rows())
                    {
                        if (isFirstRow)
                        {
                            var headerValues = row.Cells().Select(x => x.Value.ToString());
                            headerColCnt = headerValues.Count();
                            isFirstRow = false; // 첫 번째 행 처리 완료
                            csvContent.AppendLine(string.Join(",", headerValues));
                            continue;
                        }

                        // 각 셀(cell)을 순차적으로 처리하고, 값 가져오기
                        var cellValues = new List<string>();
                        for (var colNum = 1; colNum <= headerColCnt; colNum++)
                        {
                            // 셀 값을 CSV 형식으로 추가 (콤마로 구분)
                            var cell = row.Cell(colNum);
                            var cellValue = cell.Value.ToString().Replace(",", ""); // 콤마 포함 방지
                            cellValues.Add(cellValue);
                        }

                        if (cellValues.Count > 0)
                        {
                            if (cellValues.First().StartsWith("#"))
                            {
                                // 주석 제외
                                continue;
                            }
                        }

                        // 첫 번째 행이 아니면 줄바꿈 추가
                        csvContent.AppendLine(string.Join(",", cellValues));
                    }

                    // 워크시트 처리 후 빈 줄 추가 (워크시트 간 구분)
                    csvContent.AppendLine();

                    // 워크시트 인덱스 증가
                    worksheetIndex++;
                }

                // CSV 파일로 저장
                var dirPath = Path.GetDirectoryName(csvFilePath);
                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.WriteAllText(csvFilePath, csvContent.ToString());
            }

            Console.WriteLine($"XSLX -> CSV: {excelFilePath} -> {csvFilePath}");
        }
    }
}

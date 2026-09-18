using System.Windows.Documents;
using ClosedXML.Excel;
using CTA.Models;
namespace CTA.Services;

public class ExcelService
{
    public List<StudentRecord> Read(string filePath)
    {
        var result = new List<StudentRecord>();
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        var headers = new Dictionary<string, int>();

        foreach (var cell in worksheet.Row(1).CellsUsed())
        {
            var header = cell.GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[header] = cell.Address.ColumnNumber;
            }
        }

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var record = new StudentRecord()
            {
                Place = GetValue(row, headers, "место"),
                Contest = GetValue(row, headers, "Конкурс"),
                LastName = GetValue(row, headers, "Фамилия"),
                FirstName = GetValue(row, headers, "Имя"),
                Teacher = GetValue(row, headers, "ФИО преподавателя"),
                RegistrationNumber = GetValue(row, headers, "рег_номер_участника"),
                SchoolName = GetValue(row, headers, "название ОУ")
            };
            result.Add(record);
        }
        return result;
    }

    private static string GetValue(
        IXLRow row,
        Dictionary<string, int> headers,
        string columnName)
    {
        if (!headers.TryGetValue(columnName, out var column))
            return "";
        return row.Cell(column).GetString().Trim();
    }
}
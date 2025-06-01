using Natiga.Models;
using OfficeOpenXml;
using System.Globalization;

namespace Natiga.Services;

public class ResultService ()
{
    public async Task<StudentResultVM?> SearchBySeatNo(string seatNo, string filePath)
    {

        ExcelPackage.License.SetNonCommercialPersonal("Ahmed Suror");

        using var stream = File.OpenRead(filePath);
        using var package = new ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets.First();

        var headers = new List<string>();
        for (int col = 1; col <= sheet.Dimension.End.Column; col++)
        {
            headers.Add(sheet.Cells[1, col].Text.Trim().ToLower());
        }

        for (int row = 2; row <= sheet.Dimension.End.Row; row++)
        {
            var currentSeatNo = sheet.Cells[row, headers.IndexOf("رقم الجلوس") + 1].Text.Trim();
            if (currentSeatNo.Equals(seatNo, StringComparison.OrdinalIgnoreCase))
            {
                var result = new StudentResultVM
                {
                    SeatNo = currentSeatNo,
                    Name = sheet.Cells[row, headers.IndexOf("اسم التلميذ") + 1].Text
                };

                for (int col = 1; col <= sheet.Dimension.End.Column; col++)
                {
                    var header = headers[col - 1];
                    if (header != "رقم الجلوس" && header != "اسم التلميذ")
                    {
                        result.Marks[CultureInfo.CurrentCulture.TextInfo.ToTitleCase(header)] =
                            sheet.Cells[row, col].Text;
                    }
                }

                return result;
            }
        }

        return null;
    }
}

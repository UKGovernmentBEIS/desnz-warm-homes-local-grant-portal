using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic.FileIO;

namespace WhlgPortalWebsite.BusinessLogic.Helpers;

public static class FileConversionHelper
{
    public static MemoryStream ConvertCsvToXlsx(Stream csvStream)
    {
        if (csvStream.CanSeek)
            csvStream.Position = 0;

        var xlsxStream = new MemoryStream();

        using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: true);
        using var parser = new TextFieldParser(reader);
        parser.TextFieldType = FieldType.Delimited;
        parser.Delimiters = new[] { "," };
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = true;

        using (var spreadsheet = SpreadsheetDocument.Create(xlsxStream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            uint rowIndex = 1;
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null) continue;

                var row = new Row { RowIndex = rowIndex++ };

                foreach (var value in fields)
                {
                    Cell cell;

                    if (IsValidDate(value, out var dateValue))
                    {
                        var oaDate = dateValue.ToOADate();
                        cell = new Cell
                        {
                            CellValue = new CellValue(oaDate.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.Number,
                            StyleIndex = 1
                        };
                    }
                    else
                    {
                        cell = new Cell
                        {
                            DataType = CellValues.InlineString,
                            InlineString = new InlineString
                            {
                                Text = new Text(value)
                            }
                        };
                    }

                    row.Append(cell);
                }

                sheetData.Append(row);
            }

            AddStylesheet(workbookPart);

            workbookPart.Workbook.Save();
        }

        xlsxStream.Position = 0;
        return xlsxStream;
    }

    private static bool IsValidDate(string input, out DateTime date)
    {
        return DateTime.TryParseExact(input,
            ["yyyy-MM-dd HH:mm:ss"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private static void AddStylesheet(WorkbookPart workbookPart)
    {
        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

        var numberingFormats = new NumberingFormats { Count = 1 };
        var customDateFormat = new NumberingFormat
        {
            NumberFormatId = 164,
            FormatCode = StringValue.FromString("dd\\/MM\\/yyyy HH:mm:ss")
        };
        numberingFormats.Append(customDateFormat);

        var fonts = new Fonts(new Font());
        var fills = new Fills(new Fill());
        var borders = new Borders(new Border());
        var cellStyleFormats = new CellStyleFormats(new CellFormat());

        var cellFormats = new CellFormats();
        cellFormats.Append(new CellFormat()); // Index 0 - default
        cellFormats.Append(new CellFormat
        {
            NumberFormatId = 164,
            ApplyNumberFormat = true
        });

        stylesPart.Stylesheet = new Stylesheet
        {
            NumberingFormats = numberingFormats,
            Fonts = fonts,
            Fills = fills,
            Borders = borders,
            CellStyleFormats = cellStyleFormats,
            CellFormats = cellFormats
        };

        stylesPart.Stylesheet.Save();
    }
}
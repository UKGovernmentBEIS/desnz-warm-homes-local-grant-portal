using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic.FileIO;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public class StreamService : IStreamService
{
    private class CsvReferralRequest
    {
        [Name("Referral date")] public string Date { get; }
        [Optional] [Name("Referral code")] public string Code { get; set; }
        [Optional] public string Name { get; set; }
        [Optional] public string Email { get; set; }
        [Optional] public string Telephone { get; set; }
        [Optional] public string Address1 { get; set; }
        [Optional] public string Address2 { get; set; }
        [Optional] public string Town { get; set; }
        [Optional] public string County { get; set; }
        [Optional] public string Postcode { get; set; }
        [Optional] [Name("UPRN")] public string Uprn { get; set; }
        [Optional] [Name("EPC Band")] public string EpcBand { get; set; }

        [Optional]
        [Name("EPC confirmed by homeowner")]
        public string EpcConfirmedByHomeowner { get; set; }

        [Optional]
        [Name("EPC Lodgement Date")]
        public string EpcLodgementDate { get; set; }

        [Optional]
        [Name("Household income band")]
        public string HouseholdIncomeBand { get; set; }

        [Optional]
        [Name("Is eligible postcode")]
        public string IsEligiblePostcode { get; set; }

        [Optional] public string Tenure { get; set; }

        [Optional] // optional as it doesnt appear in input csv
        [Name("Custodian Code")]
        public string CustodianCode { get; set; }

        [Optional] [Name("Local Authority")] public string LocalAuthority { get; set; }
    }

    public MemoryStream ConvertCsvToXlsx(Stream csvStream)
    {
        if (csvStream.CanSeek)
            csvStream.Position = 0;

        var xlsxStream = new MemoryStream();

        using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: true);
        using var parser = new TextFieldParser(reader);
        parser.TextFieldType = FieldType.Delimited;
        parser.Delimiters = [","];
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = true;

        using (var spreadsheet = SpreadsheetDocument.Create(xlsxStream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            };
            sheets.Append(sheet);

            uint rowIndex = 1;
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null) continue;
                var row = new Row { RowIndex = rowIndex };
                rowIndex++;

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

    public async Task<Stream> ConvertLocalAuthorityS3StreamsIntoConsortiumStream(
        Dictionary<string, Stream> csvFileStreams)
    {
        var referralRequests = new List<CsvReferralRequest>();
        foreach (var kvp in csvFileStreams)
        {
            var localAuthorityName = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[kvp.Key];
            using var reader = new StreamReader(kvp.Value);
            using var localAuthorityCsv = new CsvReader(reader, CultureInfo.InvariantCulture);
            referralRequests.AddRange(localAuthorityCsv
                .GetRecords<CsvReferralRequest>()
                .Select(record =>
                {
                    record.CustodianCode = kvp.Key;
                    record.LocalAuthority = localAuthorityName;
                    return record;
                })
            );
        }

        referralRequests = referralRequests
            .Select(referralRequest => (DateTime.Parse(referralRequest.Date), referralRequest))
            .OrderBy(dateAndReferralRequest => dateAndReferralRequest.Item1)
            .Select(dateAndReferralRequest => dateAndReferralRequest.referralRequest)
            .ToList();

        byte[] outBytes;

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(referralRequests);
        await writer.FlushAsync();

        outBytes = stream.ToArray();
        return new MemoryStream(outBytes);
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
        cellFormats.Append(new CellFormat());
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
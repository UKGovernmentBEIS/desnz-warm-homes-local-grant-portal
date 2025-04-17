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

    public Stream ConvertCsvToXlsx(Stream csvStream)
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
            // This section is somewhat boilerplate, and can be found here:
            // https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/how-to-create-a-spreadsheet-document-by-providing-a-file-name
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

        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(referralRequests);
        await writer.FlushAsync();

        return new MemoryStream(stream.ToArray());
    }

    private static bool IsValidDate(string input, out DateTime date)
    {
        return DateTime.TryParseExact(input,
            ["yyyy-MM-dd HH:mm:ss"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    /// <summary>
    ///     Adds a custom stylesheet for the custom date format
    ///     This stylesheet will extend the basic styles available within excel to add an extra custom one
    /// </summary>
    /// <param name="workbookPart"></param>
    private static void AddStylesheet(WorkbookPart workbookPart)
    {
        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

        // Create a custom number format for our date to be formatted as "dd/MM/yyyy HH:mm:ss"
        // We have to predefine here how many we will be including in this stylesheet, and so the count is set to 1
        var numberingFormats = new NumberingFormats { Count = 1 };
        const uint dateFormatId = 164; // 164 is used, as 1-163 are reserved IDs
        var customDateFormat = new NumberingFormat
        {
            NumberFormatId = dateFormatId,
            FormatCode = StringValue.FromString("dd\\/MM\\/yyyy HH:mm:ss")
        };
        numberingFormats.Append(customDateFormat);

        // This section initialises each part of the stylesheet we need
        // but which we have to add 1 empty entry to for it to apply correctly
        var fonts = new Fonts(new Font());
        var fills = new Fills(new Fill());
        var borders = new Borders(new Border());
        var cellStyleFormats = new CellStyleFormats(new CellFormat());

        // Define a new cell format which we will add the new number format we made previously to
        var cellFormats = new CellFormats();
        cellFormats.Append(new CellFormat());
        cellFormats.Append(new CellFormat
        {
            NumberFormatId = dateFormatId,
            ApplyNumberFormat = true // This flag tells Excel to use the variable above
        });

        // Add our custom number format & cell format that uses it (and the other defaults)
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
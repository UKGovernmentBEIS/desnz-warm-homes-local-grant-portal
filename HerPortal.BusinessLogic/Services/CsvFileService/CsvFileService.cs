using System.Globalization;
using System.Security;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using HerPortal.BusinessLogic.Models;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;

namespace HerPortal.BusinessLogic.Services.CsvFileService;

public class CsvFileService : ICsvFileService
{
    private readonly IDataAccessProvider dataAccessProvider;
    private readonly S3ReferralFileKeyService keyService;
    private readonly IS3FileReader s3FileReader;
    
    private class CsvReferralRequest
    {
        [Name("Referral date")]
        public string Date { get; set; }
        [Name("Referral code")]
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        [Name("UPRN")]
        public string Uprn { get; set; }
        [Name("EPC Band")]
        public string EpcBand { get; set; }
        [Name("EPC confirmed by homeowner")]
        public string EpcConfirmedByHomeowner { get; set; }
        [Name("EPC Lodgement Date")]
        public string EpcLodgementDate { get; set; }
        [Name("Is off gas grid")]
        public string IsOffGasGrid { get; set; }
        [Name("Household income band")]
        public string HouseholdIncomeBand { get; set; }
        [Name("Is eligible postcode")]
        public string IsEligiblePostcode { get; set; }
        public string Tenure { get; set; }
        [Optional] // optional as it doesnt appear in input csv
        [Name("Custodian Code")]
        public string CustodianCode { get; set; }
    }

    public CsvFileService
    (
        IDataAccessProvider dataAccessProvider,
        S3ReferralFileKeyService keyService,
        IS3FileReader s3FileReader
    ) {
        this.dataAccessProvider = dataAccessProvider;
        this.keyService = keyService;
        this.s3FileReader = s3FileReader;
    }

    public async Task<IEnumerable<CsvFileData>> GetFileDataForUserAsync(string userEmailAddress)
    {
        // Make sure that we only return file data for files that the user currently has access to
        var user = await dataAccessProvider.GetUserByEmailAsync(userEmailAddress);
        var currentCustodianCodes = user.LocalAuthorities.Select(la => la.CustodianCode);
        
        var downloads = await dataAccessProvider.GetCsvFileDownloadDataForUserAsync(user.Id);
        var files = new List<CsvFileData>();

        var consortiumCodes = dataAccessProvider.GetConsortiumCodesForUser(user);

        foreach (var custodianCode in currentCustodianCodes)
        {
            var s3Objects = await s3FileReader.GetS3ObjectsByCustodianCodeAsync(custodianCode);
            files.AddRange(s3Objects.Select(s3O =>
                {
                    var data = keyService.GetDataFromS3Key(s3O.Key);
                    var downloadData = downloads.SingleOrDefault(d =>
                        d.CustodianCode == data.CustodianCode
                        && d.Year == data.Year
                        && d.Month == data.Month
                    );
                    return new LocalAuthorityCsvFileData
                    (
                        data.CustodianCode,
                        data.Month,
                        data.Year,
                        s3O.LastModified,
                        downloadData?.LastDownloaded
                    );
                }
            ));
        }

        files.AddRange(
            files
                .Where(file => LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode.ContainsKey(file.Code))
                .GroupBy(file => (LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode[file.Code], file.Month,
                    file.Year))
                .Where(grouping => consortiumCodes.Contains(grouping.Key.Item1))
                .Select(grouping => new ConsortiumCsvFileData(
                        grouping.Key.Item1, 
                        grouping.Key.Month, 
                        grouping.Key.Year, 
                        grouping
                            .Select(fileData => fileData.LastUpdated)
                            .Max(), // there are csv updates as new as
                        grouping
                            .Select(fileData => fileData.LastDownloaded)
                            .Min() // there are csvs undownloaded for as long as
                    )
                )
            );

        return files
            .OrderByDescending(f => new DateOnly(f.Year, f.Month, 1))
            .ThenByDescending(f => f is ConsortiumCsvFileData)
            .ThenBy(f => f.Name);
    }

    // Page number starts at 1
    public async Task<PaginatedFileData> GetPaginatedFileDataForUserAsync(
        string userEmailAddress,
        List<string> custodianCodes,
        int pageNumber,
        int pageSize)
    {
        var allFileData = (await GetFileDataForUserAsync(userEmailAddress)).ToList();
        var filteredFileData = allFileData
            .Where(cfd => custodianCodes.Count == 0 || custodianCodes.Contains(cfd.Code))
            .ToList();

        var maxPage = ((filteredFileData.Count - 1) / pageSize) + 1;
        var currentPage = Math.Min(pageNumber, maxPage);

        return new PaginatedFileData
        {
            FileData = filteredFileData.Skip((currentPage - 1) * pageSize).Take(pageSize),
            CurrentPage = currentPage,
            MaximumPage = maxPage,
            UserHasUndownloadedFiles = allFileData.Any(cf => cf.HasUpdatedSinceLastDownload)
        };
    }

    public async Task<Stream> GetLocalAuthorityFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress)
    {
        // Important! First ensure the logged-in user is allowed to access this data
        var userData = await dataAccessProvider.GetUserByEmailAsync(userEmailAddress);
        if (!userData.LocalAuthorities.Any(la => la.CustodianCode == custodianCode))
        {
            // We don't want to log the User's email address for GDPR reasons, but the ID is fine.
            throw new SecurityException(
                $"User {userData.Id} is not permitted to access file for custodian code: {custodianCode} year: {year} month: {month}.");
        }
        
        if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(custodianCode))
        {
            throw new ArgumentOutOfRangeException(nameof(custodianCode), custodianCode,
                "Given custodian code is not valid");
        }

        var fileStream = await s3FileReader.ReadFileAsync(custodianCode, year, month);
        
        // Notably, we can't confirm a download, so it's possible that we mark a file as downloaded
        //   but the user has some sort of issue and doesn't get it
        // We put this line as late as possible in the method for this reason
        await dataAccessProvider.MarkCsvFileAsDownloadedAsync(custodianCode, year, month, userData.Id);

        return fileStream;
    }

    public async Task<Stream> GetConsortiumFileForDownloadAsync(string consortiumCode, int year, int month, string userEmailAddress)
    {
        // Important! First ensure the logged-in user is allowed to access this data
        var userData = await dataAccessProvider.GetUserByEmailAsync(userEmailAddress);
        var consortiumCodes = dataAccessProvider.GetConsortiumCodesForUser(userData);

        if (!consortiumCodes.Contains(consortiumCode))
        {
            // We don't want to log the User's email address for GDPR reasons, but the ID is fine.
            throw new SecurityException(
                $"User {userData.Id} is not permitted to access file for consortium code: {consortiumCode} year: {year} month: {month}.");
        }
        
        if (!ConsortiumData.ConsortiumNamesByConsortiumCode.ContainsKey(consortiumCode))
        {
            throw new ArgumentOutOfRangeException(nameof(consortiumCode), consortiumCode,
                "Given consortium code is not valid");
        }

        var referralRequests = new List<CsvReferralRequest>();
        
        foreach (var custodianCode in ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode[consortiumCode])
        {
            var localAuthorityFile = await GetLocalAuthorityFileForDownloadAsync(custodianCode, year, month, userEmailAddress);

            using var reader = new StreamReader(localAuthorityFile);
            using var localAuthorityCsv = new CsvReader(reader, CultureInfo.InvariantCulture);
            referralRequests.AddRange(localAuthorityCsv
                .GetRecords<CsvReferralRequest>()
                .Select(record =>
                {
                    record.CustodianCode = custodianCode;
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
}
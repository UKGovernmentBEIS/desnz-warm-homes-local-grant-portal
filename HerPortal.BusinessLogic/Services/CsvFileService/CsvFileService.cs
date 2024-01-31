using System.Security;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using HerPortal.BusinessLogic.Models;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;

namespace HerPortal.BusinessLogic.Services.CsvFileService;

public class CsvFileService : ICsvFileService
{
    private readonly IDataAccessProvider dataAccessProvider;
    private readonly S3ReferralFileKeyService keyService;
    private readonly IS3FileReader s3FileReader;
    private readonly UserService userService;

    public CsvFileService
    (
        IDataAccessProvider dataAccessProvider,
        S3ReferralFileKeyService keyService,
        IS3FileReader s3FileReader,
        UserService userService
    ) {
        this.dataAccessProvider = dataAccessProvider;
        this.keyService = keyService;
        this.s3FileReader = s3FileReader;
        this.userService = userService;
    }

    public async Task<IEnumerable<AbstractCsvFileData>> GetFileDataForUserAsync(string userEmailAddress)
    {
        // Make sure that we only return file data for files that the user currently has access to
        var user = await userService.GetUserByEmailAsync(userEmailAddress);
        var currentCustodianCodes = user.LocalAuthorities.Select(la => la.CustodianCode);
        
        var downloads = await dataAccessProvider.GetCsvFileDownloadDataForUserAsync(user.Id);
        var files = new List<AbstractCsvFileData>();

        var consortiumIds = userService.GetConsortiumIdsForUser(user);

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
                .Where(file => LocalAuthorityData.LocalAuthorityConsortiumIdByCustodianCode.ContainsKey(file.Code))
                .GroupBy(file => (LocalAuthorityData.LocalAuthorityConsortiumIdByCustodianCode[file.Code], file.Month,
                    file.Year))
                .Where(grouping => consortiumIds.Contains(grouping.Key.Item1))
                .Select(grouping => new ConsortiumCsvFileData(
                        grouping.Key.Item1, 
                        grouping.Key.Month, 
                        grouping.Key.Year, 
                        grouping
                            .Select(fileData => fileData.LastUpdated)
                            .Max(), // find latest update of any LAs
                        DateTime.Now.AddDays(-5) // TODO last download database
                    )
                )
            );

        // TODO bold consortium & put them at top of date
        return files
            .OrderByDescending(f => new DateOnly(f.Year, f.Month, 1))
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

    public async Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress)
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
}
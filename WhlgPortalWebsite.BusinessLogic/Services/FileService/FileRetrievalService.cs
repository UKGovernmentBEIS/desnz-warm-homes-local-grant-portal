using System.Security;
using WhlgPortalWebsite.BusinessLogic.ExternalServices.S3FileReader;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;

namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public class FileRetrievalService(
    IDataAccessProvider dataAccessProvider,
    S3ReferralFileKeyService keyService,
    IS3FileReader s3FileReader,
    IStreamService streamService)
    : IFileRetrievalService
{

    public async Task<IEnumerable<FileData>> GetFileDataForUserAsync(string userEmailAddress)
    {
        // Make sure that we only return file data for files that the user currently has access to
        var user = await dataAccessProvider.GetUserByEmailAsync(userEmailAddress);
        var custodianCodes = user.GetAdministeredCustodianCodes();
        var consortiumCodes = user.GetAdministeredConsortiumCodes();
        
        var localAuthoritiesFileData = await BuildCsvFileDataForLocalAuthorities(user, custodianCodes);
        var consortiaTransformedFileData = TransformFileDataForConsortia(consortiumCodes, localAuthoritiesFileData);
        var combinedFileData = localAuthoritiesFileData.Concat(consortiaTransformedFileData);

        return combinedFileData
            .OrderByDescending(f => new DateOnly(f.Year, f.Month, 1))
            .ThenByDescending(f => f is ConsortiumFileData)
            .ThenBy(f => f.Name);
    }

    private async Task<List<FileData>> BuildCsvFileDataForLocalAuthorities(User user, IEnumerable<string> currentCustodianCodes)
    {
        var laFileData = new List<FileData>();
        var downloads = await dataAccessProvider.GetCsvFileDownloadDataForUserAsync(user.Id);
        foreach (var custodianCode in currentCustodianCodes)
        {
            var s3Objects = await s3FileReader.GetS3ObjectsByCustodianCodeAsync(custodianCode);
            laFileData.AddRange(s3Objects.Select(s3O =>
                {
                    var data = keyService.GetDataFromS3Key(s3O.Key);
                    var downloadData = downloads.SingleOrDefault(d =>
                        d.CustodianCode == data.CustodianCode
                        && d.Year == data.Year
                        && d.Month == data.Month
                    );
                    return new LocalAuthorityFileData
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

        return laFileData;
    }
    
    private IEnumerable<FileData> TransformFileDataForConsortia(IEnumerable<string> consortiumCodes, IEnumerable<FileData> laFileData)
    {
        return laFileData
            .Where(fileRow => LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode.ContainsKey(fileRow.Code))
            .GroupBy(fileRow => (LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode[fileRow.Code], fileRow.Month,
                fileRow.Year))
            .Where(grouping => consortiumCodes.Contains(grouping.Key.Item1))
            .Select(grouping => new ConsortiumFileData(
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
            );
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
        if (!userData.GetAdministeredCustodianCodes().Contains(custodianCode))
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

        if (!userData.GetAdministeredConsortiumCodes().Contains(consortiumCode))
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

        var s3FileStreams = new Dictionary<string, Stream>(); //Custodian Code and corresponding Stream

        foreach (var custodianCode in ConsortiumData.ConsortiumCustodianCodesIdsByConsortiumCode[consortiumCode])
        {
            if (!await s3FileReader.FileExistsAsync(custodianCode, year, month))
            {
                continue;
            }
            s3FileStreams.Add(
                custodianCode, await GetLocalAuthorityFileForDownloadAsync(custodianCode, year, month, userEmailAddress)
            );
        }

        return await streamService.ConvertLocalAuthorityS3StreamsIntoConsortiumStream(s3FileStreams);
    }
}
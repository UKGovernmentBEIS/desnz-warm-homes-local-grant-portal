namespace WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

public interface ICsvFileService
{
    public Task<IEnumerable<CsvFileData>> GetFileDataForUserAsync(string userEmailAddress);

    public Task<PaginatedFileData> GetPaginatedFileDataForUserAsync(
        string userEmailAddress,
        List<string> custodianCodes,
        int pageNumber,
        int pageSize);
    
    public Task<Stream> GetLocalAuthorityFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress);
    
    public Task<Stream> GetConsortiumFileForDownloadAsync(string consortiumCode, int year, int month, string userEmailAddress);
}
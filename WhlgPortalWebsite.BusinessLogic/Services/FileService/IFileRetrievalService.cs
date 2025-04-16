using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public interface IFileRetrievalService
{
    public Task<IEnumerable<FileData>> GetFileDataForUserAsync(string userEmailAddress);

    public Task<PaginatedFileData> GetPaginatedFileDataForUserAsync(
        string userEmailAddress,
        List<string> custodianCodes,
        int pageNumber,
        int pageSize);
    
    public Task<Stream> GetLocalAuthorityFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress);
    
    public Task<Stream> GetConsortiumFileForDownloadAsync(string consortiumCode, int year, int month, string userEmailAddress);
}
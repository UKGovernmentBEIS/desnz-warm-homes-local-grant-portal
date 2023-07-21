namespace HerPortal.BusinessLogic.Services.CsvFileService;

public interface ICsvFileService
{
    public Task<IEnumerable<CsvFileData>> GetFileDataForUserAsync(string userEmailAddress);
    public Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress);
}
namespace HerPortal.BusinessLogic.Services.CsvFileService;

public interface ICsvFileService
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodesAsync(IEnumerable<string> custodianCodes, int userId);
    public Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, string userEmailAddress);
}
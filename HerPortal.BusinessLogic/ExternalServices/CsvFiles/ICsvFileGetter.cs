namespace HerPortal.ExternalServices.CsvFiles;

public interface ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodesAsync(IEnumerable<string> custodianCodes, int userId);
    public Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, int userId);
}

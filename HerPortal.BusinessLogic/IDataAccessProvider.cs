using HerPortal.BusinessLogic.Models;

namespace HerPortal.Data;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
    public Task<CsvFileDownload> GetLastCsvFileDownloadAsync(string custodianCode, int year, int month, int userId);
    public Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId);
}

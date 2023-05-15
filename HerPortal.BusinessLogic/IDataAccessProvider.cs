using HerPortal.BusinessLogic.Models;

namespace HerPortal.Data;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
    public Task<bool> DoesCsvFileDownloadDataExistAsync(string custodianCode, int year, int month);
    public Task<CsvFileDownloadData> GetCsvFileDownloadDataAsync(string custodianCode, int year, int month);
    public Task BeginTrackingCsvFileDownloadsAsync(string custodianCode, int year, int month);
    public Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId);
}

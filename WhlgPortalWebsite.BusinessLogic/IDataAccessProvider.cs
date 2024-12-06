using HerPortal.BusinessLogic.Models;

namespace HerPortal.BusinessLogic;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
    public Task<IEnumerable<User>> GetAllActiveUsersAsync();
    public Task<List<CsvFileDownload>> GetCsvFileDownloadDataForUserAsync(int userId);
    public Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId);
}

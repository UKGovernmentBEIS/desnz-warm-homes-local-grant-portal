using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
    public Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync();
    public Task<List<CsvFileDownload>> GetCsvFileDownloadDataForUserAsync(int userId);
    public Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId);
    public Task<IList<User>> GetAllDeliveryPartnersAsync();
    public Task<IList<User>> GetAllDeliveryPartnersWhereEmailContainsAsync(string partialEmailAddress);
    public Task CreateDeliveryPartnerAsync(string userEmailAddress);
    public Task<bool> IsEmailAddressInUseAsync(string emailAddress);
}
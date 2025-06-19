using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.BusinessLogic;

public interface IDataAccessProvider
{
    public Task<User> GetUserByEmailAsync(string emailAddress);
    public Task<User> GetUserByIdAsync(int userId);
    public Task MarkUserAsHavingLoggedInAsync(int userId);
    public Task<IEnumerable<User>> GetAllActiveDeliveryPartnersAsync();
    public Task<List<CsvFileDownload>> GetCsvFileDownloadDataForUserAsync(int userId);
    public Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId);
    public Task<IList<User>> GetAllDeliveryPartnersAsync();
    public Task<IList<User>> GetAllDeliveryPartnersWhereEmailContainsAsync(string partialEmailAddress);
    public Task<User> CreateDeliveryPartnerAsync(string userEmailAddress);
    public Task<bool> IsEmailAddressInUseAsync(string emailAddress);
    public Task<IEnumerable<LocalAuthority>> GetAllLasAsync();
    public Task<IEnumerable<Consortium>> GetAllConsortiaAsync();
    public Task AddLaToDeliveryPartnerAsync(User user, LocalAuthority localAuthority);
    public Task AddConsortiumToDeliveryPartnerAsync(User user, Consortium consortium);
    public Task<LocalAuthority> GetLocalAuthorityByCustodianCodeAsync(string custodianCode);
    public Task<Consortium> GetConsortiumByConsortiumCodeAsync(string consortiumCode);
}
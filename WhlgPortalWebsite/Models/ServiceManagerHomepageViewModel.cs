using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.Enums;

namespace WhlgPortalWebsite.Models;

public class ServiceManagerHomepageViewModel
{
    public string SearchEmailAddress { get; }
    public IEnumerable<AuthorityUserListing> UserList { get; }
    public bool ShowTaskSuccess { get; set; }
    public string TaskSuccessText { get; set; }
    public bool ShowManualJobRunner { get; set; }

    public ServiceManagerHomepageViewModel(IEnumerable<User> users, TaskSuccessMessage? taskSuccessMessage = null,
        bool showManualJobRunner = false)
    {
        SearchEmailAddress = "";
        UserList = users
            .OrderBy(user => user.EmailAddress)
            .Select(user => new AuthorityUserListing(user));
        ShowManualJobRunner = showManualJobRunner;

        if (taskSuccessMessage.HasValue)
        {
            ShowTaskSuccess = true;
            TaskSuccessText = taskSuccessMessage.Value.Parse();
        }
        else
        {
            ShowTaskSuccess = false;
            TaskSuccessText = string.Empty;
        }
    }

    public class AuthorityUserListing
    {
        public int Id { get; }
        public string EmailAddress { get; }
        public string Manages { get; }

        public AuthorityUserListing(User user)
        {
            Id = user.Id;
            EmailAddress = user.EmailAddress;
            var authorityNames = new List<string>();
            authorityNames.AddRange(user.Consortia.Select(la =>
                $"{ConsortiumData.ConsortiumNamesByConsortiumCode[la.ConsortiumCode]} (Consortium)"));
            authorityNames.AddRange(user.LocalAuthorities.Select(la =>
                LocalAuthorityData.LocalAuthorityNamesByCustodianCode[la.CustodianCode]));
            Manages = string.Join(", ", authorityNames);
        }
    }
}
using System.Collections.Generic;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.Models;

public class AssignCodesToDeliveryPartnerViewModel
{
    public User User { get; set; }
    public string SearchTerm { get; set; }
    public List<LocalAuthority> LocalAuthorities { get; set; }
    public List<Consortium> Consortia { get; set; }
}
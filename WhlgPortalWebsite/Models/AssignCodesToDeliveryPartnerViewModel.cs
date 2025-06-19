using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.Models;

public class AssignCodesToDeliveryPartnerViewModel
{
    public User User { get; set; }
    public string SearchTerm { get; set; }
    public List<AssignAuthorityViewModel> LocalAuthoritiesToAssign { get; set; }
    public List<AssignAuthorityViewModel> ConsortiaToAssign { get; set; }

    public class AssignAuthorityViewModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool AlreadyAssigned { get; set; }
    }
}
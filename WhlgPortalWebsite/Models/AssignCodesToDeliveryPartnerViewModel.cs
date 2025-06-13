using System.Collections.Generic;
using System.Linq;
using WhlgPortalWebsite.BusinessLogic.Models;

namespace WhlgPortalWebsite.Models;

public class AssignCodesToDeliveryPartnerViewModel
{
    public User User { get; set; }
    public string SearchTerm { get; set; }
    public List<LocalAuthority> LocalAuthorities { get; set; }
    public List<Consortium> Consortia { get; set; }

    public IEnumerable<AssignAuthorityViewModel> GetLocalAuthoritiesToAssign()
    {
        return LocalAuthorities.Select(localAuthority => new AssignAuthorityViewModel
        {
            Name = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[localAuthority.CustodianCode],
            Code = localAuthority.CustodianCode,
            AlreadyAssigned = User.LocalAuthorities.Any(userLocalAuthority => userLocalAuthority.CustodianCode == localAuthority.CustodianCode)
        });
    }

    public IEnumerable<AssignAuthorityViewModel> GetConsortiaToAssign()
    {
        return Consortia.Select(consortium => new AssignAuthorityViewModel
        {
            Name = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[consortium.ConsortiumCode],
            Code = consortium.ConsortiumCode,
            AlreadyAssigned = User.Consortia.Any(userConsortium => userConsortium.ConsortiumCode == consortium.ConsortiumCode)
        });
    }
    
    public class AssignAuthorityViewModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool AlreadyAssigned { get; set; }
    }
}
namespace HerPortal.BusinessLogic.Models;

/// <summary>
///     This model is not a full model for Local Authority Data.
///     It should be considered as a reference to the full data for a Local Authority.
///     Data for LA Relationships to Consortiums can be found in <see cref="LocalAuthorityData"> LocalAuthorityData.cs </see>
///     The full data for LAs (including an LAs relationship to a Consortium) can be found in  the HUG2 Public Website codebase.
///     <seealso href="https://github.com/UKGovernmentBEIS/desnz-home-energy-retrofit-beta/blob/develop/HerPublicWebsite.BusinessLogic/Models/LocalAuthorityData.cs">Link to HUG2 Public Website codebase's LocalAuthorityData.cs</seealso>
/// </summary>
public class LocalAuthority
{
    public int Id { get; set; }
    public string CustodianCode { get; set; }
    
    public List<User> Users { get; set; }
}

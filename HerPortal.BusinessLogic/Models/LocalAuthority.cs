namespace HerPortal.BusinessLogic.Models;

/// <summary>
/// This model is not a full model for Local Authority Data.
/// It should be considered as a reference to the full data for a Local Authority.
/// The full data (including an LAs relationship to a Consortium) can be found in the HUG2 Public Website codebase
/// </summary>
public class LocalAuthority
{
    public int Id { get; set; }
    public string CustodianCode { get; set; }
    
    public List<User> Users { get; set; }
}

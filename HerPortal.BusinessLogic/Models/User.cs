namespace HerPortal.BusinessLogic.Models;

public class User
{
    public int Id { get; set; }
    public string EmailAddress { get; set; }
    public bool HasLoggedIn { get; set; }
    
    public List<string> AccessibleLocalAuthorityCustodianCodes { get; set; }
}

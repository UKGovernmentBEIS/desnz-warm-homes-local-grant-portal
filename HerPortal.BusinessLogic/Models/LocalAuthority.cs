namespace HerPortal.BusinessLogic.Models;

public class LocalAuthority
{
    public int Id { get; set; }
    public string CustodianCode { get; set; }
    
    public List<User> Users { get; set; }
}

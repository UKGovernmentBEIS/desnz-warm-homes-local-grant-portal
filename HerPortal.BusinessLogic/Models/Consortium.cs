namespace HerPortal.BusinessLogic.Models;

/// <summary>
/// This model is not a full model for Consortium Data.
/// It should be considered as a reference to the full data for a Consortium.
/// The full data (including an the Consortium's relationship to LAs) can be found in the HUG2 Public Website codebase
/// </summary>
public class Consortium
{
    public int Id { get; set; }
    public string ConsortiumCode { get; set; }
    
    public List<User> Users { get; set; }
}
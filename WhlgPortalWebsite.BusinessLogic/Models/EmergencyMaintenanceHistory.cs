namespace WhlgPortalWebsite.BusinessLogic.Models;

public enum EmergencyMaintenanceState
{
    Enabled,
    Disabled
}

public class EmergencyMaintenanceHistory : IEntityWithRowVersioning
{
    public int Id { get; set; }

    public EmergencyMaintenanceState State { get; set; }

    public DateTime ChangeDate { get; set; }

    public string AuthorEmail { get; set; }
    public uint Version { get; set; }
}
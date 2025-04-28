namespace WhlgPortalWebsite.BusinessLogic.Models;

public interface IEntityWithRowVersioning
{
    uint Version { get; set; }
}
namespace WhlgPortalWebsite.ManagementShell;

public interface IOutputProvider
{
    public void Output(string outputString);

    public bool Confirm(string outputString);

    public string? GetString(string outputString);
}
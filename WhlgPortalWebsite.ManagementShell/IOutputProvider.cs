namespace HerPortal.ManagementShell;

public interface IOutputProvider
{
    public void Output(string outputString);

    public bool Confirm(string outputString);
}
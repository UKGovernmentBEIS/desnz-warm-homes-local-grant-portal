namespace WhlgPortalWebsite.ManagementShell;

public class OutputProvider : IOutputProvider
{
    public void Output(string outputString)
    {
        Console.WriteLine(outputString);
    }

    public bool Confirm(string outputString)
    {
        Console.WriteLine(outputString);
        var inputString = Console.ReadLine();
        return inputString?.Trim().ToLower() == "y";
    }
}
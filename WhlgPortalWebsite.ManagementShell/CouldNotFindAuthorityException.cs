namespace HerPortal.ManagementShell;

public class CouldNotFindAuthorityException : Exception
{
    public readonly List<string> InvalidCodes;

    public CouldNotFindAuthorityException(string message, List<string> invalidCodes) : base(message)
    {
        InvalidCodes = invalidCodes;
    }
}
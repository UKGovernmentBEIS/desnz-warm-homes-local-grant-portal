using System;

namespace WhlgPortalWebsite.Enums;

public enum AuthorityType
{
    LocalAuthority,
    Consortium
}

public static class AuthorityTypeExtensions
{
    public static string Parse(this AuthorityType authorityType)
    {
        return authorityType switch
        {
            AuthorityType.LocalAuthority => "Local Authority",
            AuthorityType.Consortium => "Consortium",
            _ => throw new ArgumentOutOfRangeException(nameof(authorityType), authorityType, null)
        };
    }
}
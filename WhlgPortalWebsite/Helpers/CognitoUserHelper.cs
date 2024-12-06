using System.Linq;
using System.Security.Claims;

namespace WhlgPortalWebsite.Helpers;

public static class CognitoUserHelper
{
    public static string GetEmailAddress(this ClaimsPrincipal user)
    {
        return user.Claims
            .Single(c => c.Type == ClaimTypes.Email)
            .Value;
    }
}

using System.Linq;
using System.Security.Claims;

namespace HerPortal.Helpers;

public static class CognitoUserHelper
{
    public static string GetEmailAddress(this ClaimsPrincipal user)
    {
        return user.Claims
            .Single(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
            .Value;
    }
}

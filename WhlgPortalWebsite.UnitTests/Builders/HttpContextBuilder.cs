using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Tests.Builders;

public class HttpContextBuilder
{
    private HttpContext context;
    
    public HttpContextBuilder(string emailAddress)
    {
        var claims = new List<Claim>() 
        {
            new(ClaimTypes.Email, emailAddress),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(identity);
    }

    public HttpContext Build()
    {
        return context;
    }
}
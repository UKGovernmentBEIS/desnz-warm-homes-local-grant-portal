using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Helpers;

namespace WhlgPortalWebsite.Filters;

public class RequiresServiceManagerFilter(IUserService userService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var emailAddress = context.HttpContext.User.GetEmailAddress();
        var user = await userService.GetUserByEmailAsync(emailAddress);

        if (user.Role != UserRole.ServiceManager)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
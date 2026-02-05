using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Helpers;

namespace WhlgPortalWebsite.Filters;

public class RequiresServiceManagerFilter(IUserService userService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Exceptions aren't handled if thrown in an AuthorizationFilter class
        // Therefore, we have to handle them manually.
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-10.0#authorization-filters

        var emailAddress = context.HttpContext.User.GetEmailAddress();

        User user;
        try
        {
            user = await userService.GetUserByEmailAsync(emailAddress);
        }
        catch (Exception ex) when (ex.IsUserNotFoundException())
        {
            // This might happen only if the user is deleted from the database but not from Cognito
            context.Result = new RedirectToActionResult("DeletedUser", "Error", null);

            return;
        }

        if (user.Role != UserRole.ServiceManager)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
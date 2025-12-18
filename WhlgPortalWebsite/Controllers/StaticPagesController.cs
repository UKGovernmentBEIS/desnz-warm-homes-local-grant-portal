using WhlgPortalWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Controllers;

public class StaticPagesController : Controller
{
    [HttpGet("/privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View("PrivacyPolicy");
    }
}
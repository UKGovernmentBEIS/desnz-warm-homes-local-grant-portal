using WhlgPortalWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Controllers;

public class StaticPagesController : Controller
{
    [HttpGet("/accessibility-statement")]
    public IActionResult AccessibilityStatement()
    {
        return View("AccessibilityStatement");
    }
    
    [HttpGet("/privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View("PrivacyPolicy");
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerPortal.Controllers;

public class StaticPagesController : Controller
{
    [Authorize]
    [HttpGet("/")]
    public IActionResult Index()
    {
        return View("Index");
    }

    [HttpGet("/sign-in")]
    public IActionResult SignIn()
    {
        return RedirectPermanent("https://desnz-her-portal-sandbox.auth.eu-west-2.amazoncognito.com/oauth2/authorize?client_id=5ksfss8daoeoo34hmq71svvc03&response_type=code&scope=email+openid+phone&redirect_uri=https://localhost:5001");
    }
    
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
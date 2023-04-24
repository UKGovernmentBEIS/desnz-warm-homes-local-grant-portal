using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HerPortal.Controllers;

[Route("sign-out")]
public class SignOutController : Controller
{
    public IActionResult Index()
    {
        HttpContext.SignOutAsync();

        return RedirectToAction("Index", "StaticPages");
    }
}
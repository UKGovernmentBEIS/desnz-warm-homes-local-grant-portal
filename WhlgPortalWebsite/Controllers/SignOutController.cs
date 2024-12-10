using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Controllers;

[Route("sign-out")]
public class SignOutController : Controller
{
    public async Task<IActionResult> Index()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}

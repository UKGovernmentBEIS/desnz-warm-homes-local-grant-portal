using System.Threading.Tasks;
using HerPortal.DataStores;
using HerPortal.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HerPortal.Controllers;

public class HomeController : Controller
{
    private readonly UserDataStore userDataStore;
    private readonly ILogger<HomeController> logger;

    public HomeController(UserDataStore userDataStore, ILogger<HomeController> logger)
    {
        this.userDataStore = userDataStore;
        this.logger = logger;
    }
    
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var principal = HttpContext.User;
        var emailAddress = principal.GetEmailAddress();
        var userData = await userDataStore.GetUserByEmailAsync(emailAddress);
        ViewBag.UserData = userData;
        return View("Index");
    }
}
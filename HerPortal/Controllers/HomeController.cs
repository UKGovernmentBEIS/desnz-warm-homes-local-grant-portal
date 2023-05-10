using System.Threading.Tasks;
using HerPortal.DataStores;
using HerPortal.Helpers;
using HerPortal.Models;
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
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userDataStore.GetUserByEmailAsync(userEmailAddress);
        var homepageViewModel = new HomepageViewModel(userData);
        if (!userData.HasLoggedIn)
        {
            await userDataStore.MarkUserAsHavingLoggedInAsync(userData.Id);
        }
        return View("Index", homepageViewModel);
    }
}
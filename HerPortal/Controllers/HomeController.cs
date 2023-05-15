using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HerPortal.DataStores;
using HerPortal.ExternalServices.CsvFiles;
using HerPortal.Helpers;
using HerPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HerPortal.Controllers;

public class HomeController : Controller
{
    private readonly UserDataStore userDataStore;
    private readonly ICsvFileGetter csvFileGetter;
    private readonly ILogger<HomeController> logger;

    public HomeController
    (
        UserDataStore userDataStore,
        ICsvFileGetter csvFileGetter,
        ILogger<HomeController> logger
    ) {
        this.userDataStore = userDataStore;
        this.csvFileGetter = csvFileGetter;
        this.logger = logger;
    }
    
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userDataStore.GetUserByEmailAsync(userEmailAddress);

        var csvFiles = await csvFileGetter.GetByCustodianCodesAsync
        (
            userData.LocalAuthorities.Select(la => la.CustodianCode)
        );
        
        var homepageViewModel = new HomepageViewModel(userData, csvFiles);
        if (!userData.HasLoggedIn)
        {
            await userDataStore.MarkUserAsHavingLoggedInAsync(userData.Id);
        }
        return View("Index", homepageViewModel);
    }
}

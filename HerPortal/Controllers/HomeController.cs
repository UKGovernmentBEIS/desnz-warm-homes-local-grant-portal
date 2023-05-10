using System;
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

        var csvFiles = (await csvFileGetter.GetByCustodianCodes
        (
            userData.LocalAuthorities.Select(la => la.CustodianCode)
        )).Select(cf => new HomepageViewModel.CsvFile
        (
            new DateOnly(cf.Year, cf.Month, 1).ToString("MMMM yyyy"),
            userData
                .LocalAuthorities
                .Single(la => la.CustodianCode == cf.CustodianCode)
                .Name,
            cf.LastUpdated.ToString("dd/MM/yy"),
            cf.HasUpdatedSinceLastDownload,
            cf.HasApplications
        ));
        
        var homepageViewModel = new HomepageViewModel(userData, csvFiles);
        if (!userData.HasLoggedIn)
        {
            await userDataStore.MarkUserAsHavingLoggedInAsync(userData.Id);
        }
        return View("Index", homepageViewModel);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.DataStores;
using HerPortal.Helpers;
using HerPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HerPortal.Controllers;

public class HomeController : Controller
{
    private readonly UserDataStore userDataStore;
    private readonly ICsvFileService csvFileService;
    private readonly ILogger<HomeController> logger;

    public HomeController
    (
        UserDataStore userDataStore,
        ICsvFileService csvFileService,
        ILogger<HomeController> logger
    ) {
        this.userDataStore = userDataStore;
        this.csvFileService = csvFileService;
        this.logger = logger;
    }
    
    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] List<string> custodianCodes)
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userDataStore.GetUserByEmailAsync(userEmailAddress);

        var allUserCustodianCodes = userData.LocalAuthorities.Select(la => la.CustodianCode);

        var allUserCsvFiles = (await csvFileService
            .GetByCustodianCodesAsync(allUserCustodianCodes, userData.Id))
            .ToList();
        var filteredCsvFiles = allUserCsvFiles;

        if (custodianCodes.Count > 0)
        {
            filteredCsvFiles = filteredCsvFiles
                .Where(cf => custodianCodes.Contains(cf.CustodianCode))
                .ToList();
        }
        
        var homepageViewModel = new HomepageViewModel
        (
            userData,
            filteredCsvFiles,
            allUserCsvFiles.Any(cf => cf.HasUpdatedSinceLastDownload)
        );
        
        if (!userData.HasLoggedIn)
        {
            await userDataStore.MarkUserAsHavingLoggedInAsync(userData.Id);
        }
        return View("ReferralFiles", homepageViewModel);
    }

    [HttpGet("/supporting-documents")]
    public IActionResult SupportingDocuments()
    {
        return View("SupportingDocuments");
    }
}

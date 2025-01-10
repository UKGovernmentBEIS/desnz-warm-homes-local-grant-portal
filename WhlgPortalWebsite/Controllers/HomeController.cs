using System.Collections.Generic;
using System.Threading.Tasks;
using WhlgPortalWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.Models;

namespace WhlgPortalWebsite.Controllers;

public class HomeController : Controller
{
    private readonly UserService userService;
    private readonly ICsvFileService csvFileService;
    private const int PageSize = 20;

    public HomeController
    (
        UserService userService,
        ICsvFileService csvFileService
    ) {
        this.userService = userService;
        this.csvFileService = csvFileService;
    }
    
    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] List<string> codes, int page = 1)
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userService.GetUserByEmailAsync(userEmailAddress);

        var csvFilePage = await csvFileService.GetPaginatedFileDataForUserAsync(userEmailAddress, codes, page, PageSize);

        string GetPageLink(int pageNumber) => Url.Action(nameof(Index), "Home", new RouteValueDictionary() { { "custodianCodes", codes }, { "page", pageNumber } });

        string GetDownloadLink(CsvFileData abstractCsvFileData)
        {
            return abstractCsvFileData switch
            {
                LocalAuthorityCsvFileData localAuthorityCsvFileData => Url.Action(
                    nameof(CsvFileController.GetLaCsvFile), "CsvFile",
                    new RouteValueDictionary()
                    {
                        { "custodianCode", localAuthorityCsvFileData.Code },
                        { "year", localAuthorityCsvFileData.Year },
                        { "month", localAuthorityCsvFileData.Month }
                    }),
                ConsortiumCsvFileData consortiumCsvFileData => Url.Action(nameof(CsvFileController.GetConsortiumCsvFile),
                    "CsvFile",
                    new RouteValueDictionary()
                    {
                        { "consortiumCode", consortiumCsvFileData.Code },
                        { "year", consortiumCsvFileData.Year },
                        { "month", consortiumCsvFileData.Month }
                    }),
                _ => ""
            };
        }

        var homepageViewModel = new HomepageViewModel
        (
            userData,
            csvFilePage,
            GetPageLink,
            GetDownloadLink
        );

        if (!userData.HasLoggedIn)
        {
            await userService.MarkUserAsHavingLoggedInAsync(userData.Id);
        }
        return View("ReferralFiles", homepageViewModel);
    }

    [HttpGet("/supporting-documents")]
    public IActionResult SupportingDocuments()
    {
        return View("SupportingDocuments");
    }
}

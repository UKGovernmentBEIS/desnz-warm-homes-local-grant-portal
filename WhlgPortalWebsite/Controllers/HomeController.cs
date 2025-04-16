using System.Collections.Generic;
using System.Threading.Tasks;
using WhlgPortalWebsite.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Models;

namespace WhlgPortalWebsite.Controllers;

public class HomeController : Controller
{
    private readonly UserService userService;
    private readonly IFileRetrievalService fileRetrievalService;
    private const int PageSize = 20;

    public HomeController
    (
        UserService userService,
        IFileRetrievalService fileRetrievalService
    ) {
        this.userService = userService;
        this.fileRetrievalService = fileRetrievalService;
    }
    
    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] List<string> codes, int page = 1)
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userService.GetUserByEmailAsync(userEmailAddress);

        var csvFilePage = await fileRetrievalService.GetPaginatedFileDataForUserAsync(userEmailAddress, codes, page, PageSize);

        string GetPageLink(int pageNumber) => Url.Action(nameof(Index), "Home", new RouteValueDictionary { { "custodianCodes", codes }, { "page", pageNumber } });

        string GetDownloadLink(FileData abstractFileData, FileType fileType)
        {
            return abstractFileData switch
            {
                LocalAuthorityFileData localAuthorityFileData => Url.Action(
                    nameof(FileController.GetLaFile), "File",
                    new RouteValueDictionary
                    {
                        { "custodianCode", localAuthorityFileData.Code },
                        { "year", localAuthorityFileData.Year },
                        { "month", localAuthorityFileData.Month },
                        { "fileExtension", fileType.ToString().ToLower()}
                    }),
                ConsortiumFileData consortiumFileData => Url.Action(nameof(FileController.GetConsortiumFile),
                    "File",
                    new RouteValueDictionary
                    {
                        { "consortiumCode", consortiumFileData.Code },
                        { "year", consortiumFileData.Year },
                        { "month", consortiumFileData.Month },
                        { "fileExtension", fileType.ToString().ToLower()}
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

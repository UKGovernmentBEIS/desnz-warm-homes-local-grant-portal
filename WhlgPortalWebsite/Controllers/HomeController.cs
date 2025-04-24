using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Helpers;
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
    )
    {
        this.userService = userService;
        this.fileRetrievalService = fileRetrievalService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] List<string> codes, int page = 1)
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userService.GetUserByEmailAsync(userEmailAddress);

        return userData.Role switch
        {
            UserRole.AuthorityStaff => await AuthorityStaffIndex(codes, page, userEmailAddress, userData),
            UserRole.ServiceManager => ServiceManagerIndex(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<IActionResult> AuthorityStaffIndex(List<string> codes, int page, string userEmailAddress, User userData)
    {
        var csvFilePage =
            await fileRetrievalService.GetPaginatedFileDataForUserAsync(userEmailAddress, codes, page, PageSize);

        string GetPageLink(int pageNumber)
        {
            return Url.Action(nameof(Index), "Home",
                new RouteValueDictionary { { "custodianCodes", codes }, { "page", pageNumber } });
        }

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
                        { "fileExtension", fileType.ToString().ToLower() }
                    }),
                ConsortiumFileData consortiumFileData => Url.Action(nameof(FileController.GetConsortiumFile),
                    "File",
                    new RouteValueDictionary
                    {
                        { "consortiumCode", consortiumFileData.Code },
                        { "year", consortiumFileData.Year },
                        { "month", consortiumFileData.Month },
                        { "fileExtension", fileType.ToString().ToLower() }
                    }),
                _ => ""
            };
        }

        var homepageViewModel = new AuthorityStaffHomepageViewModel
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

        return View("AuthorityStaff/ReferralFiles", homepageViewModel);
    }

    private IActionResult ServiceManagerIndex()
    {
        return View("ServiceManager/Index");
    }

    [HttpGet("/supporting-documents")]
    public IActionResult SupportingDocuments()
    {
        return View("AuthorityStaff/SupportingDocuments");
    }
}
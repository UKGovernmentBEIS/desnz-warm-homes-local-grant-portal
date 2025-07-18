﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Models.Enums;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Helpers;
using WhlgPortalWebsite.Models;

namespace WhlgPortalWebsite.Controllers;

public class HomeController(
    IUserService userService,
    IFileRetrievalService fileRetrievalService,
    IWebHostEnvironment webHostEnvironment)
    : Controller
{
    private const int PageSize = 20;

    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] List<string> codes, [FromQuery] string searchEmailAddress,
        [FromQuery] bool jobSuccess,
        int page = 1)
    {
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userService.GetUserByEmailAsync(userEmailAddress);

        return userData.Role switch
        {
            UserRole.DeliveryPartner => await RenderDeliveryPartnerHomepage(codes, page, userEmailAddress, userData),
            UserRole.ServiceManager => await RenderServiceManagerHomepage(searchEmailAddress, jobSuccess),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<IActionResult> RenderDeliveryPartnerHomepage(List<string> codes, int page,
        string userEmailAddress,
        User userData)
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

        var homepageViewModel = new DeliveryPartnerHomepageViewModel
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

        return View("DeliveryPartner/ReferralFiles", homepageViewModel);
    }

    private async Task<IActionResult> RenderServiceManagerHomepage(string searchEmailAddress, bool jobSuccess)
    {
        var users = await userService.SearchAllDeliveryPartnersAsync(searchEmailAddress);

        var homepageViewModel = new ServiceManagerHomepageViewModel(users)
        {
            ShowManualJobRunner = !webHostEnvironment.IsProduction(),
            ShowJobSuccess = jobSuccess
        };

        return View("ServiceManager/Index", homepageViewModel);
    }

    [HttpGet("/supporting-documents")]
    public IActionResult SupportingDocuments()
    {
        return View("DeliveryPartner/SupportingDocuments");
    }
}
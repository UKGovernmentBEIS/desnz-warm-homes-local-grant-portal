using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Filters;
using WhlgPortalWebsite.Models;

namespace WhlgPortalWebsite.Controllers;

[TypeFilter(typeof(RequiresServiceManagerFilter))]
[Route("service-manager")]
public class ServiceManagerController(
    IUserService userService,
    IAuthorityService authorityService,
    IReminderEmailsService reminderEmailsService) : Controller
{
    [HttpGet("onboard-delivery-partner")]
    public IActionResult OnboardDeliveryPartner_Get()
    {
        return View("OnboardDeliveryPartner", new OnboardNewDeliveryPartnerViewModel());
    }

    [HttpPost("onboard-delivery-partner")]
    public async Task<IActionResult> OnboardDeliveryPartner_Post(
        OnboardNewDeliveryPartnerViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("OnboardDeliveryPartner", viewModel);
        }

        var emailAddress = viewModel.EmailAddress.Trim();

        if (await userService.IsEmailAddressInUseAsync(emailAddress))
        {
            ModelState.AddModelError(nameof(viewModel.EmailAddress), "This email address is already in use.");
            return View("OnboardDeliveryPartner", viewModel);
        }

        var newUser = await userService.CreateDeliveryPartnerAsync(emailAddress);
        return RedirectToAction("AssignCodesToDeliveryPartner_Get", "ServiceManager",
            new { userId = newUser.Id });
    }

    [HttpGet("assign-codes-to-delivery-partner/{userId:int}")]
    public async Task<IActionResult> AssignCodesToDeliveryPartner_Get([FromRoute] int userId,
        [FromQuery] string searchTerm)
    {
        var user = await userService.GetUserByIdAsync(userId);
        var localAuthorities = (await authorityService.SearchAllLasAsync(searchTerm)).ToList();
        var consortia = (await authorityService.SearchAllConsortiaAsync(searchTerm)).ToList();

        var viewModel = new AssignCodesToDeliveryPartnerViewModel
        {
            User = user,
            LocalAuthoritiesToAssign = localAuthorities.Select(localAuthority =>
                new AssignCodesToDeliveryPartnerViewModel.AssignAuthorityViewModel
                {
                    Name = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[localAuthority.CustodianCode],
                    Code = localAuthority.CustodianCode,
                    AlreadyAssigned = authorityService.UserManagesLocalAuthority(user, localAuthority)
                }
            ).ToList(),
            ConsortiaToAssign = consortia.Select(consortium =>
                new AssignCodesToDeliveryPartnerViewModel.AssignAuthorityViewModel
                {
                    Name = ConsortiumData.ConsortiumNamesByConsortiumCode[consortium.ConsortiumCode],
                    Code = consortium.ConsortiumCode,
                    AlreadyAssigned = authorityService.UserManagesConsortium(user, consortium)
                }).ToList(),
            SearchTerm = searchTerm
        };

        return View("AssignCodesToDeliveryPartner", viewModel);
    }

    [HttpGet("confirm-add-authority-to-delivery-partner/{userId:int}/{authorityType}/{code}")]
    public async Task<IActionResult> ConfirmAuthorityCodeToDeliveryPartner_Get([FromRoute] int userId,
        [FromRoute] AuthorityType authorityType, [FromRoute] string code)
    {
        var managedLocalAuthorityCodes = authorityType switch
        {
            AuthorityType.LocalAuthority => [code],
            AuthorityType.Consortium =>
                ConsortiumData
                    .ConsortiumCustodianCodesIdsByConsortiumCode[code],
            _ => throw new InvalidOperationException("Unknown authority type")
        };

        var viewModel = new ConfirmCodesToDeliveryPartnerViewModel
        {
            User = await userService.GetUserByIdAsync(userId),
            Code = code,
            ManagedLocalAuthorityCodes = managedLocalAuthorityCodes,
            AuthorityType = authorityType
        };

        return View("ConfirmCodeToDeliveryPartner", viewModel);
    }

    [HttpPost("confirm-add-authority-to-delivery-partner/{userId:int}/{authorityType}/{code}")]
    public async Task<IActionResult> ConfirmAuthorityCodeToDeliveryPartner_Post(
        ConfirmCodesToDeliveryPartnerViewModel viewModel, [FromRoute] int userId,
        [FromRoute] AuthorityType authorityType, [FromRoute] string code)
    {
        if (!ModelState.IsValid)
        {
            return await ConfirmAuthorityCodeToDeliveryPartner_Get(userId, authorityType, code);
        }

        var user = await userService.GetUserByIdAsync(userId);

        switch (authorityType)
        {
            case AuthorityType.LocalAuthority:
            {
                var localAuthority = await authorityService.GetLocalAuthorityByCustodianCodeAsync(code);
                await userService.AddLaToDeliveryPartnerAsync(user, localAuthority);
                break;
            }
            case AuthorityType.Consortium:
            {
                var consortium = await authorityService.GetConsortiumByConsortiumCodeAsync(code);
                await userService.AddConsortiumToDeliveryPartnerAsync(user, consortium);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(authorityType), authorityType.ToString());
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("send-reminder-emails")]
    public async Task<IActionResult> SendReminderEmails_Post()
    {
        await reminderEmailsService.SendReminderEmailsAsync();

        return RedirectToAction(nameof(HomeController.Index), "Home", new { jobSuccess = true });
    }
}
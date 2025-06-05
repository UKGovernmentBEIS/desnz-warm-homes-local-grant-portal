using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Filters;
using WhlgPortalWebsite.Models;

namespace WhlgPortalWebsite.Controllers;

[TypeFilter(typeof(RequiresServiceManagerFilter))]
[Route("service-manager")]
public class ServiceManagerController(IUserService userService) : Controller
{
    [HttpGet("onboard-delivery-partner")]
    public IActionResult OnboardDeliveryPartnerPage()
    {
        return View("OnboardDeliveryPartner", new OnboardNewDeliveryPartnerViewModel());
    }

    [HttpPost("onboard-delivery-partner")]
    public async Task<IActionResult> OnboardDeliveryPartnerAsync(
        OnboardNewDeliveryPartnerViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("OnboardDeliveryPartner", viewModel);
        }

        if (await userService.IsEmailAddressInUseAsync(viewModel.EmailAddress))
        {
            ModelState.AddModelError(nameof(viewModel.EmailAddress), "This email address is already in use.");
            return View("OnboardDeliveryPartner", viewModel);
        }

        await userService.CreateDeliveryPartnerAsync(viewModel.EmailAddress);
        return RedirectToAction("Index", "Home");
    }
}
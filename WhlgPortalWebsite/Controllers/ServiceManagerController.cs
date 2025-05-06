using Microsoft.AspNetCore.Mvc;
using WhlgPortalWebsite.Filters;

namespace WhlgPortalWebsite.Controllers;

[TypeFilter(typeof(RequiresServiceManagerFilter))]
[Route("service-manager")]
public class ServiceManagerController : Controller
{
    // TODO: PC-1841/PC-1842 replace this page with service manager specific views
    [HttpGet("test-page")]
    public IActionResult ServiceManagerTestPage()
    {
        return View("Test");
    }
}
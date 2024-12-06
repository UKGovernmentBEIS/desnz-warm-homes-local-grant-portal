using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerPortal.Controllers
{
    public class HealthCheckController : Controller
    {
        [HttpGet("/health-check")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}

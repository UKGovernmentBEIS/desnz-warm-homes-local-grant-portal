﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Controllers
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

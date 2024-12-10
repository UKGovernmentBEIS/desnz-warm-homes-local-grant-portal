﻿using HerPortal.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HerPortal.Controllers;

public class StaticPagesController : Controller
{
    [HttpGet("/accessibility-statement")]
    public IActionResult AccessibilityStatement()
    {
        return View("AccessibilityStatement");
    }
    
    [HttpGet("/privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View("PrivacyPolicy");
    }
}
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WhlgPortalWebsite.Controllers;

[Route("error")]
public class ErrorController: Controller
{
    [HttpGet]
    [HttpPost]
    [AllowAnonymous]
    public IActionResult HandleException()
    {
        return View("ServiceIssue");
    }
    
    [HttpGet("{code:int}")]
    public IActionResult HandleErrorsWithStatusCode(int code)
    {
        return code switch
        {
            404 => View("PageNotFound"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

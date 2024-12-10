﻿using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPortal.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HerPortal.Controllers;

[Route("/download")]
public class CsvFileController : Controller
{
    private readonly ICsvFileService csvFileService;
    private readonly ILogger<CsvFileController> logger;

    public CsvFileController
    (
        ICsvFileService csvFileService,
        ILogger<CsvFileController> logger
    ) {
        this.csvFileService = csvFileService;
        this.logger = logger;
    }
    
    [HttpGet("/la/{custodianCode}/{year:int}/{month:int}")]
    public async Task<IActionResult> GetLaCsvFile(string custodianCode, int year, int month)
    {
        return await HandleAccessingFile(
            async () => await csvFileService.GetLocalAuthorityFileForDownloadAsync(custodianCode, year, month, HttpContext.User.GetEmailAddress()),
            $"{custodianCode}_{year}-{month:D2}.csv",
            $"An error occured while trying to access the CSV file with custodian code {custodianCode}, year {year}, month {month}."
        );
    }
    
    [HttpGet("/consortium/{consortiumCode}/{year:int}/{month:int}")]
    public async Task<IActionResult> GetConsortiumCsvFile(string consortiumCode, int year, int month)
    {
        return await HandleAccessingFile(
            async () => await csvFileService.GetConsortiumFileForDownloadAsync(consortiumCode, year, month, HttpContext.User.GetEmailAddress()),
            $"{consortiumCode}_{year}-{month:D2}.csv",
            $"An error occured while trying to access the CSV file with consortium code {consortiumCode}, year {year}, month {month}."
        );
    }

    private async Task<IActionResult> HandleAccessingFile(Func<Task<Stream>> fileAccessor, string fileName, string errorMessage)
    {
        Stream file;
        try
        {
            file = await fileAccessor();
        }
        catch (SecurityException ex)
        {
            // If this is happening, someone is trying to get around the access controls or there's a bug
            logger.LogWarning(ex.Message);
            return Unauthorized("The logged-in user is not permitted to access this resource.");
        }
        catch (ArgumentOutOfRangeException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError
            (
                ex,
                "{errorMessage}",
                errorMessage
            );
            return Problem(errorMessage);
        }

        return File(file, "text/csv", fileName);
    }
}

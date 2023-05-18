using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HerPortal.DataStores;
using HerPortal.ExternalServices.CsvFiles;
using HerPortal.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HerPortal.Controllers;

[Route("/download")]
public class CsvFileController : Controller
{
    private readonly UserDataStore userDataStore;
    private readonly ICsvFileGetter csvFileGetter;
    private readonly ILogger<CsvFileController> logger;

    public CsvFileController
    (
        UserDataStore userDataStore,
        ICsvFileGetter csvFileGetter,
        ILogger<CsvFileController> logger
    ) {
        this.userDataStore = userDataStore;
        this.csvFileGetter = csvFileGetter;
        this.logger = logger;
    }
    
    [HttpGet("{custodianCode}/{year:int}/{month:int}")]
    public async Task<IActionResult> GetCsvFile(string custodianCode, int year, int month)
    {
        // Important! First ensure the logged-in user is allowed to access this data
        var userEmailAddress = HttpContext.User.GetEmailAddress();
        var userData = await userDataStore.GetUserByEmailAsync(userEmailAddress);
        if (!userData.LocalAuthorities.Any(la => la.CustodianCode == custodianCode))
        {
            return Unauthorized("The logged-in user is not permitted to access this resource.");
        }

        Stream file;
        try
        {
            file = await csvFileGetter.GetFileForDownloadAsync(custodianCode, year, month, userData.Id);
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
                "Error encountered when attempting to get CSV file with custodian code {CustodianCode}, year {Year}, month {Month}",
                custodianCode,
                year,
                month
            );
            return Problem($"An error occured while trying to access the CSV file with custodian code {custodianCode}, year {year}, month {month}.");
        }

        return File(file, "text/csv", $"{custodianCode}_{year}-{month:D2}.csv");
    }
}

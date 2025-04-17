using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;
using WhlgPortalWebsite.Helpers;

namespace WhlgPortalWebsite.Controllers;

[Route("/download")]
public class FileController(
    IFileRetrievalService fileRetrievalService,
    ILogger<FileController> logger,
    IStreamService streamService)
    : Controller
{
    [HttpGet("/la/{custodianCode}/{year:int}/{month:int}/{fileExtension}")]
    public async Task<IActionResult> GetLaFile(string custodianCode, int year, int month, string fileExtension)
    {
        if (!Enum.TryParse(fileExtension, true, out FileType fileType))
        {
            throw new InvalidEnumArgumentException($"{fileExtension} is not a valid FileType");
        }

        return await HandleAccessingFile(
            async () => await fileRetrievalService.GetLocalAuthorityFileForDownloadAsync(custodianCode, year, month,
                HttpContext.User.GetEmailAddress()),
            $"{custodianCode}_{year}-{month:D2}.{fileExtension.ToLower()}",
            $"An error occured while trying to access the {fileExtension.ToUpper()} file with custodian code {custodianCode}, year {year}, month {month}.",
            fileType
        );
    }

    [HttpGet("/consortium/{custodianCode}/{year:int}/{month:int}/{fileExtension}")]
    public async Task<IActionResult> GetConsortiumFile(string consortiumCode, int year, int month, string fileExtension)
    {
        if (!Enum.TryParse(fileExtension, true, out FileType fileType))
        {
            throw new InvalidEnumArgumentException($"{fileExtension} is not a valid FileType");
        }

        return await HandleAccessingFile(
            async () => await fileRetrievalService.GetConsortiumFileForDownloadAsync(consortiumCode, year, month,
                HttpContext.User.GetEmailAddress()),
            $"{consortiumCode}_{year}-{month:D2}.{fileExtension.ToLower()}",
            $"An error occured while trying to access the {fileExtension.ToUpper()} file with consortium code {consortiumCode}, year {year}, month {month}.",
            fileType
        );
    }

    private async Task<IActionResult> HandleAccessingFile(Func<Task<Stream>> fileAccessor, string fileName,
        string errorMessage, FileType fileType)
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

        return fileType switch
        {
            FileType.Csv => File(file, "text/csv", fileName),
            FileType.Xlsx => File(streamService.ConvertCsvToXlsx(file),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName),
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
        };
    }
}
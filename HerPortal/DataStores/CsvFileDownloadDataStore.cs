using System.Collections.Generic;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;
using HerPortal.Data;
using Microsoft.Extensions.Logging;

namespace HerPortal.DataStores;

public class CsvFileDownloadDataStore
{
    private readonly IDataAccessProvider dataAccessProvider;
    private readonly ILogger<CsvFileDownloadDataStore> logger;

    public CsvFileDownloadDataStore(IDataAccessProvider dataAccessProvider, ILogger<CsvFileDownloadDataStore> logger)
    {
        this.dataAccessProvider = dataAccessProvider;
        this.logger = logger;
    }

    public async Task<List<TrackedCsvFile>> GetCsvFilesDownloadedByUserAsync(int userId)
    {
        return await dataAccessProvider.GetCsvFilesDownloadedByUserAsync(userId);
    }
    
    public async Task MarkCsvFileAsDownloadedAsync(string custodianCode, int year, int month, int userId)
    {
        logger.LogInformation
        (
            "Marking CSV file data for custodian code {CustodianCode}, year {Year}, month {Month} as having been downloaded by user {UserId}",
            custodianCode,
            year,
            month,
            userId
        );
        await dataAccessProvider.MarkCsvFileAsDownloadedAsync(custodianCode, year, month, userId);
    }
}

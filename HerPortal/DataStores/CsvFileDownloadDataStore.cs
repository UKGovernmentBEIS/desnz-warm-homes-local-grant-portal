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

    public async Task<bool> DoesCsvFileDownloadDataExistAsync(string custodianCode, int year, int month)
    {
        return await dataAccessProvider.DoesCsvFileDownloadDataExistAsync(custodianCode, year, month);
    }
    
    public async Task<CsvFileDownloadData> GetCsvFileDownloadDataAsync(string custodianCode, int year, int month)
    {
        return await dataAccessProvider.GetCsvFileDownloadDataAsync(custodianCode, year, month);
    }
    
    public async Task BeginTrackingCsvFileDownloadsAsync(string custodianCode, int year, int month)
    {
        logger.LogInformation
        (
            "Beginning tracking CSV file data for custodian code {CustodianCode}, year {Year}, month {Month}",
            custodianCode,
            year,
            month
        );
        await dataAccessProvider.BeginTrackingCsvFileDownloadsAsync(custodianCode, year, month);
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

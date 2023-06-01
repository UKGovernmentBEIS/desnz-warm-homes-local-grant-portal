using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using HerPortal.DataStores;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;

namespace HerPortal.ExternalServices.CsvFiles;

public class CsvFileGetter : ICsvFileGetter
{
    private readonly CsvFileDownloadDataStore csvFileDownloadDataStore;
    private readonly S3ReferralFileKeyService keyService;
    private readonly IS3FileReader s3FileReader;

    public CsvFileGetter
    (
        CsvFileDownloadDataStore csvFileDownloadDataStore,
        S3ReferralFileKeyService keyService,
        IS3FileReader s3FileReader
    ) {
        this.csvFileDownloadDataStore = csvFileDownloadDataStore;
        this.keyService = keyService;
        this.s3FileReader = s3FileReader;
    }
    
    public async Task<IEnumerable<CsvFileData>> GetByCustodianCodesAsync(IEnumerable<string> custodianCodes, int userId)
    {
        var downloads = await csvFileDownloadDataStore.GetLastCsvFileDownloadsAsync(userId);
        var files = new List<CsvFileData>();

        foreach (var custodianCode in custodianCodes)
        {
            var s3Objects = await s3FileReader.GetS3ObjectsByCustodianCodeAsync(custodianCode);
            files.AddRange(s3Objects.Select(s3O =>
                {
                    var data = keyService.GetDataFromS3Key(s3O.Key);
                    var downloadData = downloads.SingleOrDefault(d =>
                        d.CustodianCode == data.CustodianCode
                        && d.Year == data.Year
                        && d.Month == data.Month
                    );
                    return new CsvFileData
                    (
                        data.CustodianCode,
                        data.Month,
                        data.Year,
                        s3O.LastModified,
                        downloadData?.LastDownloaded
                    );
                }
            ));
        }

        return files
            .OrderByDescending(f => new DateOnly(f.Year, f.Month, 1))
            .ThenBy(f => LocalAuthorityData.LocalAuthorityNamesByCustodianCode[f.CustodianCode]);
    }

    public async Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, int userId)
    {
        if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(custodianCode))
        {
            throw new ArgumentOutOfRangeException(nameof(custodianCode), custodianCode,
                "Given custodian code is not valid");
        }

        var fileStream = await s3FileReader.ReadFileAsync(custodianCode, year, month);
        
        // Notably, we can't confirm a download, so it's possible that we mark a file as downloaded
        //   but the user has some sort of issue and doesn't get it
        // We put this line as late as possible in the method for this reason
        await csvFileDownloadDataStore.MarkCsvFileAsDownloadedAsync(custodianCode, year, month, userId);

        return fileStream;
    }
}

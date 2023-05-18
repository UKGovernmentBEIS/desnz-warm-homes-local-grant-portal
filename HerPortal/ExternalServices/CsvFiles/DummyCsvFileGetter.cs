using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;
using HerPortal.DataStores;

namespace HerPortal.ExternalServices.CsvFiles;

public class DummyCsvFileGetter : ICsvFileGetter
{
    private readonly CsvFileDownloadDataStore csvFileDownloadDataStore;

    public DummyCsvFileGetter(CsvFileDownloadDataStore csvFileDownloadDataStore)
    {
        this.csvFileDownloadDataStore = csvFileDownloadDataStore;
    }
    
    public async Task<IEnumerable<CsvFileData>> GetByCustodianCodesAsync(IEnumerable<string> custodianCodes, int userId)
    {
        var ccList = custodianCodes.ToList();
        
        if (!ccList.Any())
        {
            return new List<CsvFileData>();
        }

        var downloads = await csvFileDownloadDataStore.GetLastCsvFileDownloadsAsync(userId);

        // Dummy data
        var csvFiles = new List<CsvFileData>();
        const int year = 2023;
        foreach (var month in new List<int> { 4, 3, 2, 1 })
        {
            foreach (var cc in ccList)
            {
                var downloadData = downloads.SingleOrDefault(d =>
                    d.CustodianCode == cc
                    && d.Year == year
                    && d.Month == month
                );

                var csvFileData = new CsvFileData(
                    cc,
                    month,
                    year,
                    new DateTime(2023, 3, 10),
                    downloadData?.LastDownloaded,
                    true
                );
                
                csvFiles.Add(csvFileData);
            }
        }

        return csvFiles;
    }

    public async Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, int userId)
    {
        if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(custodianCode))
        {
            throw new ArgumentOutOfRangeException(nameof(custodianCode), custodianCode,
                "Given custodian code is not valid");
        }
        
        using var writeableMemoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(writeableMemoryStream, Encoding.UTF8);
        {
            await streamWriter.WriteLineAsync("Referral date,Name,Email,Telephone,Preferred contact method,Address1,Address2,Town,County,Postcode,UPRN,EPC Band,Is off gas grid,Household income band,Is eligible postcode,Tenure");
            await streamWriter.WriteLineAsync("2023-01-01 13:00:01,Full Name1,contact1@example.com,00001 123456,Email,Address 1 line 1,Address 1 line 2,Town1,County1,AL01 1RS,100 111 222 001,E,yes,Below £31k,no,Owner");
            await streamWriter.FlushAsync();
            
            var resultStream = new MemoryStream
            (
                writeableMemoryStream.GetBuffer(),
                0,
                (int)writeableMemoryStream.Length,
                false
            );
            
            // Notably, we can't confirm a download, so it's possible that we mark a file as downloaded
            //   but the user has some sort of issue and doesn't get it
            // We put this line as late as possible in the method for this reason
            await csvFileDownloadDataStore.MarkCsvFileAsDownloadedAsync(custodianCode, year, month, userId);
            
            return resultStream;
        }
    }
}

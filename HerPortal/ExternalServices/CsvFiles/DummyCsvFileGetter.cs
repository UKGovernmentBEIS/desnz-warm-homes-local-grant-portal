using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HerPortal.BusinessLogic.Models;

namespace HerPortal.ExternalServices.CsvFiles;

public class DummyCsvFileGetter : ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodes(IEnumerable<string> custodianCodes)
    {
        var ccList = custodianCodes.ToList();
        
        if (!ccList.Any())
        {
            return Task.FromResult<IEnumerable<CsvFileData>>(new List<CsvFileData>());
        }
        
        return Task.FromResult
        (
            new List<int> { 4, 3, 2, 1 }.SelectMany
            (
                month => ccList
                    .Select(cc => new CsvFileData(
                        cc,
                        month,
                        2023,
                        new DateTime(2023, 3, 10),
                        new DateTime(2023, 6 - month, 1),
                        true
                    ))
            ).Prepend(new CsvFileData(
                ccList.First(),
                5,
                2023,
                new DateTime(2023, 5, 1),
                null,
                false
            ))
        );
    }

    public async Task<Stream> GetFile(string custodianCode, int year, int month)
    {
        if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(custodianCode))
        {
            throw new ArgumentOutOfRangeException(nameof(custodianCode), custodianCode,
                "Given custodian code is not valid");
        }
        
        using var writeableMemoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(writeableMemoryStream, Encoding.UTF8);
        {
            await streamWriter.WriteLineAsync("Name,Email,Telephone,Preferred contact method,Address1,Address2,Town,County,Postcode,EPC Band,Is off gas grid,Household income band,Is eligible postcode,Tenure");
            await streamWriter.WriteLineAsync("Full Name1,contact1@example.com,00001 123456,Email,Address 1 line 1,Address 1 line 2,Town1,County1,AL01 1RS,E,yes,Below £31k,no,Owner");
            await streamWriter.FlushAsync();
            
            var resultStream = new MemoryStream
            (
                writeableMemoryStream.GetBuffer(),
                0,
                (int)writeableMemoryStream.Length,
                false
            );
            return resultStream;
        }
    }
}

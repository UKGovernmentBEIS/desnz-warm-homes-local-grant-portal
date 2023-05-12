using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}

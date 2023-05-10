using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HerPortal.ExternalServices.CsvFiles;

public class DummyCsvFileGetter : ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodes(IEnumerable<string> custodianCodes)
    {
        return Task.FromResult
        (
            new List<int> { 5, 4, 3, 2, 1 }.SelectMany
            (
                month => custodianCodes
                    .Select(cc => new CsvFileData(
                        cc,
                        month,
                        2023,
                        new DateTime(2023, 5, 10)
                    ))
            )
        );
    }
}

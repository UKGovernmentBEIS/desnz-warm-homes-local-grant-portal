using System.Collections.Generic;
using System.Threading.Tasks;

namespace HerPortal.ExternalServices.CsvFiles;

public interface ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodes(IEnumerable<string> custodianCodes);
}

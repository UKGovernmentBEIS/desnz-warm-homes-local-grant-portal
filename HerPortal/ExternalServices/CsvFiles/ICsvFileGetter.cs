using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HerPortal.ExternalServices.CsvFiles;

public interface ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodes(IEnumerable<string> custodianCodes);
    public Task<Stream> GetFile(string custodianCode, int year, int month);
}

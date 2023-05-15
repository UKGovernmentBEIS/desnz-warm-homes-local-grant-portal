using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HerPortal.ExternalServices.CsvFiles;

public interface ICsvFileGetter
{
    public Task<IEnumerable<CsvFileData>> GetByCustodianCodesAsync(IEnumerable<string> custodianCodes);
    public Task<Stream> GetFileForDownloadAsync(string custodianCode, int year, int month, int userId);
}

using Amazon.S3.Model;

namespace WhlgPortalWebsite.BusinessLogic.ExternalServices.S3FileReader;

public interface IS3FileReader
{
    public Task<Stream> ReadFileAsync(string custodianCode, int year, int month);
    public Task<IEnumerable<S3Object>> GetS3ObjectsByCustodianCodeAsync(string custodianCode);
    public Task<bool> FileExistsAsync(string custodianCode, int year, int month);
}

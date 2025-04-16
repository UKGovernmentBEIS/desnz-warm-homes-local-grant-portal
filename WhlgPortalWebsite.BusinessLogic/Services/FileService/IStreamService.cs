namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public interface IStreamService
{
    public MemoryStream ConvertCsvToXlsx(Stream csvStream);

    public Task<Stream> ConvertLocalAuthorityS3StreamsIntoConsortiumStream(Dictionary<string, Stream> csvFileStreams);
}
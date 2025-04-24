namespace WhlgPortalWebsite.BusinessLogic.Services.FileService;

public interface IStreamService
{
    public Stream ConvertCsvToXlsx(Stream csvStream);

    public Task<Stream> ConvertLocalAuthorityS3StreamsIntoConsortiumStream(Dictionary<string, Stream> csvFileStreams);
}
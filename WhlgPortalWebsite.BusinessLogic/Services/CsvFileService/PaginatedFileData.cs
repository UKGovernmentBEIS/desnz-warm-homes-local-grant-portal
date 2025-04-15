namespace WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

public class PaginatedFileData
{
    public IEnumerable<FileData> FileData { get; set; } = new List<FileData>();
    public int CurrentPage { get; set; }
    public int MaximumPage { get; set; }
    
    // This field isn't ideal to have here, but calculating it requires going through all the S3 files for a user which
    // is something that is likely to be relatively slow and something that we do anyway to get the paginated file data.
    public bool UserHasUndownloadedFiles { get; set; }
}
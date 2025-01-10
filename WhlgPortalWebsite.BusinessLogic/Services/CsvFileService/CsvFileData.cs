namespace WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

public abstract class CsvFileData
{
    public string Code { get; }
    public int Month { get; }
    public int Year { get; }
    public DateTime LastUpdated { get; }
    public DateTime? LastDownloaded { get; }
    // The below slightly complicated boolean expression evaluates to false ONLY when
    //   LastDownloaded is not null and is more recent than LastUpdated.
    // When LastDownloaded is null, we assume it hasn't been downloaded, therefore
    //   it will always have been updated since it was last downloaded.
    public bool HasUpdatedSinceLastDownload => !LastDownloaded.HasValue || LastDownloaded.Value.CompareTo(LastUpdated) < 0;
    public abstract string Name { get; }

    protected CsvFileData
    (
        string code,
        int month,
        int year,
        DateTime lastUpdated,
        DateTime? lastDownloaded
    ) {
        Code = code;
        Month = month;
        Year = year;
        LastUpdated = lastUpdated;
        LastDownloaded = lastDownloaded;
    }
}
namespace WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;

public abstract class CsvFileData
{
    // Any referrals prior to March 2025 will be considered legacy
    // This is to ensure all referrals made during Private Beta (starting March 3rd) are not considered legacy
    private const int WhlgStartYear = 2025;
    private const int WhlgStartMonth = 3;

    protected CsvFileData
    (
        string code,
        int month,
        int year,
        DateTime lastUpdated,
        DateTime? lastDownloaded
    )
    {
        Code = code;
        Month = month;
        Year = year;
        LastUpdated = lastUpdated;
        LastDownloaded = lastDownloaded;
    }

    public string Code { get; }
    public int Month { get; }
    public int Year { get; }
    public DateTime LastUpdated { get; }

    public DateTime? LastDownloaded { get; }

    // The below slightly complicated boolean expression evaluates to false ONLY when
    //   LastDownloaded is not null and is more recent than LastUpdated.
    // When LastDownloaded is null, we assume it hasn't been downloaded, therefore
    //   it will always have been updated since it was last downloaded.
    public bool HasUpdatedSinceLastDownload =>
        !LastDownloaded.HasValue || LastDownloaded.Value.CompareTo(LastUpdated) < 0;

    public bool ContainsLegacyReferrals =>
        Year < WhlgStartYear ||
        (Year == WhlgStartYear && Month < WhlgStartMonth);

    public abstract string Name { get; }
}
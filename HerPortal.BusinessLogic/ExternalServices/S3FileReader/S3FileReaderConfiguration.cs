namespace HerPortal.BusinessLogic.ExternalServices.S3FileReader;

public class S3FileReaderConfiguration
{
    public const string ConfigSection = "S3";

    public string BucketName { get; set; }

    public string Region { get; set; }
}

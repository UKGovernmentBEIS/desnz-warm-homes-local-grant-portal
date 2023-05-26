using Amazon;

namespace HerPublicWebsite.BusinessLogic.ExternalServices.S3FileWriter;

public class S3FileReaderConfiguration
{
    public const string ConfigSection = "S3";

    public string BucketName { get; set; }

    public string Region { get; set; }
}

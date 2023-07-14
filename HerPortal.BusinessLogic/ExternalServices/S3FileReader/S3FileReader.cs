using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HerPortal.BusinessLogic.ExternalServices.S3FileReader;

public class S3FileReader : IS3FileReader
{
    private readonly S3FileReaderConfiguration config;
    private readonly S3ReferralFileKeyService keyService;
    private readonly AmazonS3Client s3Client;

    private readonly ILogger logger;

    public S3FileReader
    (
        IOptions<S3FileReaderConfiguration> options,
        S3ReferralFileKeyService keyService,
        ILogger<S3FileReader> logger,
        IWebHostEnvironment environment
    ) {
        this.config = options.Value;
        this.keyService = keyService;

        this.logger = logger;

        try
        {
            if (environment.IsEnvironment("Development"))
            {
                // For local development connect to a local instance of Minio
                var clientConfig = new AmazonS3Config
                {
                    AuthenticationRegion = config.Region,
                    ServiceURL = config.LocalDevOnly_ServiceUrl,
                    ForcePathStyle = true,
                };
                s3Client = new AmazonS3Client(config.LocalDevOnly_AccessKey, config.LocalDevOnly_SecretKey, clientConfig);
            }
            else
            {
                s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(config.Region));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error encountered while connecting to Amazon S3");
            throw;
        }
    }
    
    public async Task<Stream> ReadFileAsync(string custodianCode, int year, int month)
    {
        var key = keyService.GetS3KeyFromData(custodianCode, year, month);

        try
        {
            var fileTransferUtility = new TransferUtility(s3Client);
            return await fileTransferUtility.OpenStreamAsync(config.BucketName, key);
        }
        catch (AmazonS3Exception ex)
        { 
            logger.LogError
            (
                "AWS S3 error when reading CSV file from bucket: \"{BucketName}\", key: \"{Key}\". Message: \"{Message}\"",
                config.BucketName,
                key,
                ex.Message
            );
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError
            (
                "Error when reading CSV file from bucket: \"{BucketName}\", key: \"{Key}\". Message: \"{Message}\"",
                config.BucketName,
                key,
                ex.Message
            );
            throw;
        }
    }

    public async Task<IEnumerable<S3Object>> GetS3ObjectsByCustodianCodeAsync(string custodianCode)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = config.BucketName,
                Prefix = custodianCode + "/",
            };
            var files = await s3Client.ListObjectsV2Async(request);
            return files.S3Objects.Where(s3O => keyService.IsValidS3Key(s3O.Key));
        }
        catch (AmazonS3Exception ex)
        { 
            logger.LogError
            (
                "AWS S3 error when listing CSV files from bucket: \"{BucketName}\", custodian code: \"{CustodianCode}\". Message: \"{Message}\"",
                config.BucketName,
                custodianCode,
                ex.Message
            );
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError
            (
                "Error when listing CSV files from bucket: \"{BucketName}\", custodian code: \"{CustodianCode}\". Message: \"{Message}\"",
                config.BucketName,
                custodianCode,
                ex.Message
            );
            throw;
        }
    }
}

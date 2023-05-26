using System.Text.RegularExpressions;
using HerPortal.BusinessLogic.Models;
using Microsoft.Extensions.Logging;

namespace HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;

public class S3ReferralFileKeyService
{
    private readonly ILogger logger;

    public S3ReferralFileKeyService(ILogger<S3ReferralFileKeyService> logger)
    {
        this.logger = logger;
    }

    public string GetS3KeyFromData(string custodianCode, int year, int month)
    {
        if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(custodianCode))
        {
            throw new ArgumentException("Invalid custodian code: " + custodianCode);
        }

        if (year is < 1000 or > 9999)
        {
            throw new ArgumentException("Invalid year: " + year);
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentException("Invalid month: " + month);
        }

        return $"{custodianCode}/{year}_{month:D2}.csv";
    }

    public (string CustodianCode, int Year, int Month) GetDataFromS3Key(string s3Key)
    {
        var s3KeyRegex = new Regex(@"^(?<custodianCode>\d+)/(?<year>\d+)_(?<month>\d+)\.csv$");
        
        if (!s3KeyRegex.IsMatch(s3Key))
        {
            logger.LogError("Could not extract custodian code, year, or month from S3 key \"{S3Key}\"", s3Key);
            throw new ArgumentOutOfRangeException
            (
                nameof(s3Key),
                s3Key,
                $"Could not extract custodian code, year, or month from S3 key \"{s3Key}\""
            );
        }

        var match = s3KeyRegex.Match(s3Key);
        var custodianCode = match.Groups["custodianCode"].Value;
        var year = int.Parse(match.Groups["year"].Value);
        var month = int.Parse(match.Groups["month"].Value);

        return (custodianCode, year, month);
    }
}

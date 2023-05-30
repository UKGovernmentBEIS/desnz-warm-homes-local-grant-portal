using System;
using FluentAssertions;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;

namespace Tests.BusinessLogic.Services;

[TestFixture]
public class S3ReferralFileKeyServiceTests
{
    private Mock<ILogger<S3ReferralFileKeyService>> mockLogger;
    
    [SetUp]
    public void Setup()
    {
        mockLogger = new Mock<ILogger<S3ReferralFileKeyService>>();
    }
    
    [TestCase("5210", 2023, 1, "5210/2023_01.csv")]
    [TestCase("505", 2024, 5, "505/2024_05.csv")]
    [TestCase("4215", 2020, 12, "4215/2020_12.csv")]
    public void S3ReferralFileKeyService_WhenGivenCustodianCodeYearAndMonth_ConstructsTheS3Key
    (
        string custodianCode,
        int year,
        int month,
        string expectedS3Key
    ) {
        // Arrange
        var underTest = new S3ReferralFileKeyService(mockLogger.Object);
        
        // Act
        var result = underTest.GetS3KeyFromData(custodianCode, year, month);
        
        // Assert
        result.Should().Be(expectedS3Key);
    }
    
    [TestCase("5210/2023_01.csv", "5210", 2023, 1)]
    [TestCase("505/2024_05.csv", "505", 2024, 5)]
    [TestCase("4215/2020_12.csv", "4215", 2020, 12)]
    public void S3ReferralFileKeyService_WhenGivenS3Key_ExtractsTheCustodianCodeYearAndMonth
    (
        string s3Key,
        string expectedCustodianCode,
        int expectedYear,
        int expectedMonth
    ) {
        // Arrange
        var underTest = new S3ReferralFileKeyService(mockLogger.Object);
        
        // Act
        var result = underTest.GetDataFromS3Key(s3Key);
        
        // Assert
        result.CustodianCode.Should().Be(expectedCustodianCode);
        result.Year.Should().Be(expectedYear);
        result.Month.Should().Be(expectedMonth);
    }
    
    [TestCase("a")]
    [TestCase("a.csv")]
    [TestCase("42152020_12.csv")]
    [TestCase("4215202012.csv")]
    [TestCase("42152020/12.csv")]
    [TestCase("4215/12.csv")]
    [TestCase("505/2024_05")]
    public void S3ReferralFileKeyService_WhenGivenMalformedS3Key_ThrowsArgumentOutOfRangeException
    (
        string s3Key
    ) {
        // Arrange
        var underTest = new S3ReferralFileKeyService(mockLogger.Object);
        
        // Act
        var act = () => underTest.GetDataFromS3Key(s3Key);
        
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

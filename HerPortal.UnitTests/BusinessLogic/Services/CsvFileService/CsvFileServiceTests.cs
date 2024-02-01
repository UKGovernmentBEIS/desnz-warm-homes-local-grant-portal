using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Amazon.S3.Model;
using FluentAssertions;
using HerPortal.BusinessLogic;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using HerPortal.BusinessLogic.Models;
using HerPortal.BusinessLogic.Services;
using HerPortal.BusinessLogic.Services.CsvFileService;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tests.Builders;

namespace Tests.BusinessLogic.Services.CsvFileService;

public class CsvFileServiceTests
{
    private Mock<ILogger<S3ReferralFileKeyService>> mockS3Logger;
    private Mock<IDataAccessProvider> mockDataAccessProvider;
    private Mock<IS3FileReader> mockFileReader;
    private HerPortal.BusinessLogic.Services.CsvFileService.CsvFileService underTest;
    
    [SetUp]
    public void Setup()
    {
        mockDataAccessProvider = new Mock<IDataAccessProvider>();
        mockS3Logger = new Mock<ILogger<S3ReferralFileKeyService>>();
        mockFileReader = new Mock<IS3FileReader>();
        var s3ReferralFileKeyService = new S3ReferralFileKeyService(mockS3Logger.Object);

        underTest = new HerPortal.BusinessLogic.Services.CsvFileService.CsvFileService(mockDataAccessProvider.Object, s3ReferralFileKeyService, mockFileReader.Object);
    }

    [Test]
    public void GetCsvFile_WhenCalledForUnauthorisedCustodianCode_ThrowsSecurityException()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider
            .Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress))
            .ReturnsAsync(user);
        
        // Act and Assert
        Assert.ThrowsAsync<SecurityException>(async () => await underTest.GetLocalAuthorityFileForDownloadAsync("115", 2020, 01, "test@example.com"));
    }

    [Test]
    public async Task GetFileDataForUserAsync_WhenCalledWithNeverDownloadedFiles_ReturnsFileData()
    {
        // Arrange
        var user = GetUserWithLas("114", "910");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>());
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_01.csv", LastModified = new DateTime(2023, 01, 31) },
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };
        var s3Objects910 = new List<S3Object>
        {
            new() { Key = "910/2023_01.csv", LastModified = new DateTime(2023, 01, 30) },
            new() { Key = "910/2023_02.csv", LastModified = new DateTime(2023, 02, 05) },
        };
        
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("910")).ReturnsAsync(s3Objects910);

        // Act
        var result = (await underTest.GetFileDataForUserAsync(user.EmailAddress)).ToList();
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new("114", 1, 2023, new DateTime(2023, 01, 31), null),
            new("114", 2, 2023, new DateTime(2023, 02, 04), null),
            new("910", 1, 2023, new DateTime(2023, 01, 30), null),
            new("910", 2, 2023, new DateTime(2023, 02, 05), null),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task GetFileDataForUserAsync_WhenCalledWithDownloadedFile_ReturnsFileData()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);

        // Act
        var result = (await underTest.GetFileDataForUserAsync(user.EmailAddress)).ToList();
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new ("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetFileDataForUserAsync_WhenCalledWithInaccessibleFiles_ReturnsOnlyAccessibleFileData()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "910",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };
        var s3Objects910 = new List<S3Object>
        {
            new() { Key = "910/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("910")).ReturnsAsync(s3Objects910);

        // Act
        var result = (await underTest.GetFileDataForUserAsync(user.EmailAddress)).ToList();
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new ("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetPaginatedFileDataForUserAsync_WhenCalledWithOutFilters_ReturnsAllFiles()
    {
        // Arrange
        var user = GetUserWithLas("114", "910");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "910",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };
        var s3Objects910 = new List<S3Object>
        {
            new() { Key = "910/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("910")).ReturnsAsync(s3Objects910);

        // Act
        var result = (await underTest.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string>(), 1, 20));
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new ("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
            new ("910", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.FileData.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetPaginatedFileDataForUserAsync_WhenCalledWithFilters_FiltersFiles()
    {
        // Arrange
        var user = GetUserWithLas("114", "910");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "910",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };
        var s3Objects910 = new List<S3Object>
        {
            new() { Key = "910/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);
        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("910")).ReturnsAsync(s3Objects910);

        // Act
        var result = (await underTest.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string> { "114" }, 1, 20));
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new ("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.FileData.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetPaginatedFileDataForUserAsync_WhenCalledWithForPage2_ReturnsFilesForPage2()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 03, 06),
                    Month = 3,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
            new() { Key = "114/2023_03.csv", LastModified = new DateTime(2023, 03, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);

        // Act
        var result = (await underTest.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string>(), 2, 1));
        
        // Assert
        var expectedResult = new List<LocalAuthorityCsvFileData>()
        {
            new ("114", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.FileData.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetPaginatedFileDataForUserAsync_WhenCalledForUserWithUndownloadedFiles_ReturnsUserHasUndownloadedFilesAsTrue()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 08) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);

        // Act
        var result = (await underTest.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string>(), 2, 1));
        
        // Assert
        result.UserHasUndownloadedFiles.Should().BeTrue();
    }
    
    [Test]
    public async Task GetPaginatedFileDataForUserAsync_WhenCalledForUserWithoutUndownloadedFiles_ReturnsUserHasUndownloadedFilesAsFalse()
    {
        // Arrange
        var user = GetUserWithLas("114");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>());
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "114",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects114 = new List<S3Object>
        {
            new() { Key = "114/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("114")).ReturnsAsync(s3Objects114);

        // Act
        var result = (await underTest.GetPaginatedFileDataForUserAsync(user.EmailAddress, new List<string>(), 2, 1));
        
        // Assert
        result.UserHasUndownloadedFiles.Should().BeFalse();
    }
    
    [Test]
    public async Task GetFileDataForUserAsync_WhenCalledWithConsortium_ReturnsFileData()
    {
        // Arrange
        var user = GetUserWithLas("660", "665");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>{"C_0008"});
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "660",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "665",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects660 = new List<S3Object>
        {
            new() { Key = "660/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("660")).ReturnsAsync(s3Objects660);
        
        var s3Objects665 = new List<S3Object>
        {
            new() { Key = "665/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("665")).ReturnsAsync(s3Objects665);

        // Act
        var result = (await underTest.GetFileDataForUserAsync(user.EmailAddress)).ToList();
        
        // Assert
        var expectedResult = new List<AbstractCsvFileData>()
        {
            new ConsortiumCsvFileData("C_0008", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
            new LocalAuthorityCsvFileData("660", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
            new LocalAuthorityCsvFileData("665", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Test]
    public async Task GetFileDataForUserAsync_WhenCalledWithConsortium_ReturnsFileData_WithCorrectConsortiumDates()
    {
        // Arrange
        var user = GetUserWithLas("660", "665");
        
        mockDataAccessProvider.Setup(dap => dap.GetUserByEmailAsync(user.EmailAddress)).ReturnsAsync(user);
        mockDataAccessProvider.Setup(dap => dap.GetConsortiumCodesForUser(user)).Returns(new List<string>{"C_0008"});
        
        mockDataAccessProvider
            .Setup(dap => dap.GetCsvFileDownloadDataForUserAsync(user.Id))
            .ReturnsAsync(new List<CsvFileDownload>
            {
                new()
                {
                    CustodianCode = "660",
                    LastDownloaded = new DateTime(2023, 02, 10),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                },
                new()
                {
                    CustodianCode = "665",
                    LastDownloaded = new DateTime(2023, 02, 06),
                    Month = 2,
                    Year = 2023,
                    UserId = user.Id
                }
            });
        
        var s3Objects660 = new List<S3Object>
        {
            new() { Key = "660/2023_02.csv", LastModified = new DateTime(2023, 02, 02) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("660")).ReturnsAsync(s3Objects660);
        
        var s3Objects665 = new List<S3Object>
        {
            new() { Key = "665/2023_02.csv", LastModified = new DateTime(2023, 02, 04) },
        };

        mockFileReader.Setup(fr => fr.GetS3ObjectsByCustodianCodeAsync("665")).ReturnsAsync(s3Objects665);

        // Act
        var result = (await underTest.GetFileDataForUserAsync(user.EmailAddress)).ToList();
        
        // Assert
        var expectedResult = new List<AbstractCsvFileData>()
        {
            new ConsortiumCsvFileData("C_0008", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
            new LocalAuthorityCsvFileData("660", 2, 2023, new DateTime(2023, 02, 02), new DateTime(2023, 02, 10)),
            new LocalAuthorityCsvFileData("665", 2, 2023, new DateTime(2023, 02, 04), new DateTime(2023, 02, 06)),
        };
        result.Should().BeEquivalentTo(expectedResult);
    }

    
    private User GetUserWithLas(params string[] las)
    {
        return new UserBuilder("test@example.com")
            .WithLocalAuthorities(
                las.Select(la => new LocalAuthority()
                {
                    CustodianCode = la
                }).ToList())
            .Build();
    }
}
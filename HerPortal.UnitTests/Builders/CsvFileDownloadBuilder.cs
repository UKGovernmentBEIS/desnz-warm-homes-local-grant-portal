using System;
using HerPortal.BusinessLogic.Models;

namespace Tests.Builders;

public class CsvFileDownloadBuilder
{
    private CsvFileDownload csvFileDownload;

    public CsvFileDownloadBuilder(int month)
    {
        csvFileDownload = new CsvFileDownload
        {
            User = new User(),
            DateTime = new DateTime(2023, month, 1),
        };
    }

    public CsvFileDownload Build()
    {
        return csvFileDownload;
    }

    public CsvFileDownloadBuilder WithUserWithId(int userId)
    {
        csvFileDownload.User.Id = userId;
        return this;
    }
}
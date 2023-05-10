using System;

namespace HerPortal.ExternalServices.CsvFiles;

public class CsvFileData
{
    public string CustodianCode { get; }
    public int Month { get; }
    public int Year { get; }
    public DateTime LastUpdated { get; }
    
    public string S3Key => $"{CustodianCode}/{Year}_{Month:D2}.csv";

    public CsvFileData(string custodianCode, int month, int year, DateTime lastUpdated)
    {
        CustodianCode = custodianCode;
        Month = month;
        Year = year;
        LastUpdated = lastUpdated;
    }
}

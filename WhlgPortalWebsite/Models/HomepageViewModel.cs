using System;
using System.Collections.Generic;
using System.Linq;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using WhlgPortalWebsite.BusinessLogic.Models;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.BusinessLogic.Services.FileService;
using WhlgPortalWebsite.Enums;

namespace WhlgPortalWebsite.Models;

public class HomepageViewModel
{
    public HomepageViewModel(
        User user,
        PaginatedFileData paginatedFileData,
        Func<int, string> pageLinkGenerator,
        Func<FileData, FileType, string> downloadLinkGenerator
    )
    {
        var custodianCodes = user.GetAdministeredCustodianCodes().ToList();
        var consortiumCodes = user.GetAdministeredConsortiumCodes().ToList();
        var checkboxLabels = custodianCodes
            .Select(custodianCode => new KeyValuePair<string, LabelViewModel>
                (
                    custodianCode,
                    new LabelViewModel
                    {
                        Text = LocalAuthorityData.LocalAuthorityNamesByCustodianCode[custodianCode]
                    }
                )
            )
            .ToList();

        checkboxLabels.AddRange(consortiumCodes.Select(consortiumCode => new KeyValuePair<string, LabelViewModel>(
            consortiumCode,
            new LabelViewModel
            {
                Text = $"{ConsortiumData.ConsortiumNamesByConsortiumCode[consortiumCode]} (Consortium)"
            }
        )));

        ShouldShowBanner = !user.HasLoggedIn;
        ShouldShowFilters = custodianCodes.Count >= 2;
        Codes = new List<string>();
        Codes.AddRange(custodianCodes);
        Codes.AddRange(consortiumCodes);
        LocalAuthorityCheckboxLabels = new Dictionary<string, LabelViewModel>(checkboxLabels
            .OrderBy(kvp => kvp.Value.Text)
        );
        FileList = paginatedFileData.FileData.Select(cf => new ReferralDownloadListing(cf, downloadLinkGenerator(cf, FileType.Csv), downloadLinkGenerator(cf, FileType.Xlsx)));

        UserHasNewUpdates = paginatedFileData.UserHasUndownloadedFiles;

        CurrentPage = paginatedFileData.CurrentPage;
        PageUrls = Enumerable.Range(1, paginatedFileData.MaximumPage).Select(pageLinkGenerator).ToArray();
    }

    public bool ShouldShowBanner { get; }
    public bool ShouldShowFilters { get; }
    public bool UserHasNewUpdates { get; }
    public List<string> Codes { get; }
    public Dictionary<string, LabelViewModel> LocalAuthorityCheckboxLabels { get; }
    public IEnumerable<ReferralDownloadListing> FileList { get; }
    public int CurrentPage { get; }
    public string[] PageUrls { get; }
    public bool ShowLegacyColumn => FileList.Any(file => file.ContainsLegacyReferrals);

    public class ReferralDownloadListing
    {
        public ReferralDownloadListing(FileData fileData, string csvDownloadLink, string xlsxDownloadLink)
        {
            switch (fileData)
            {
                case LocalAuthorityFileData:
                    if (!LocalAuthorityData.LocalAuthorityNamesByCustodianCode.ContainsKey(fileData.Code))
                    {
                        throw new ArgumentOutOfRangeException(nameof(fileData.Code), fileData.Code,
                            "The given custodian code is not known.");
                    }

                    break;
                case ConsortiumFileData:
                    if (!ConsortiumData.ConsortiumNamesByConsortiumCode.ContainsKey(fileData.Code))
                    {
                        throw new ArgumentOutOfRangeException(nameof(fileData.Code), fileData.Code,
                            "The given consortium code is not known.");
                    }

                    break;
            }

            CustodianCode = fileData.Code;
            Year = fileData.Year;
            Month = fileData.Month;
            LastUpdatedText = fileData.LastUpdated.ToString("dd/MM/yy");
            HasNewUpdates = fileData.HasUpdatedSinceLastDownload;
            ContainsLegacyReferrals = fileData.ContainsLegacyReferrals;
            Name = fileData is ConsortiumFileData ? $"{fileData.Name} (Consortium)" : fileData.Name;
            CsvDownloadLink = csvDownloadLink;
            XlsxDownloadLink = xlsxDownloadLink;
        }
        
        public string CustodianCode { get; }
        public int Year { get; }
        public int Month { get; }
        public string MonthAndYearText => new DateOnly(Year, Month, 1).ToString("MMMM yyyy");
        public string Name { get; }
        public string LastUpdatedText { get; }
        public bool HasNewUpdates { get; }
        public bool ContainsLegacyReferrals { get; }
        public string CsvDownloadLink { get; }
        public string XlsxDownloadLink { get; }
    }
}
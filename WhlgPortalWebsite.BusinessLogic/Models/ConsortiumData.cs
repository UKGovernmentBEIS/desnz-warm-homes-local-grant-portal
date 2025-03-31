namespace WhlgPortalWebsite.BusinessLogic.Models;

/// <summary>
///     LA-Consortium mapping data can be found both in <see cref="LocalAuthorityData"/>, and in the WH:LG Public Website codebase's LocalAuthorityData.cs.
///     <seealso href="https://github.com/UKGovernmentBEIS/desnz-warm-homes-local-grant/blob/develop/WhlgPublicWebsite.BusinessLogic/Models/LocalAuthorityData.cs">Link to WH:LG Public Website codebase's LocalAuthorityData.cs</seealso>
///     Note: Mappings in the above files are expected to be consistent between both repos
/// </summary>
public static class ConsortiumData
{
    /// This dictionary data is automatically generated, see <see href="https://github.com/UKGovernmentBEIS/desnz-warm-homes-local-grant/tree/develop/scripts/local-authority-information-generators"/>.
    /// Avoid making manual changes to this code if possible
    /// We should avoid if possible changing the Consortia Code. Doing so would require us to make live database edits to update this. See <see cref="Consortium"/>.
    /// It’s much more straightforward to add new IDs and use the AddAllMissingAuthorities Portal CLI command after adding these new IDs to the codebase, then use the CLI commands to move users across.
    public static readonly Dictionary<string, string> ConsortiumNamesByConsortiumCode = new()
    {
        { "C_0001", "Blackpool Council" },
        { "C_0002", "Bristol City Council" },
        { "C_0003", "Broadland District Council" },
        { "C_0004", "Cambridge City Council" },
        { "C_0005", "Cheshire East Council" },
        { "C_0006", "Cornwall Council" },
        { "C_0007", "Darlington Borough Council (Tees Consortium)" },
        { "C_0008", "Devon County Council" },
        { "C_0009", "Dorset Council" },
        { "C_0010", "Dover District Council" },
        { "C_0011", "East Lindsey District Council" },
        { "C_0012", "Essex County Council" },
        { "C_0013", "Greater London Authority" },
        { "C_0014", "Greater Manchester Combined Authority" },
        { "C_0015", "Lewes District Council" },
        { "C_0016", "Liverpool City Region Combined Authority" },
        { "C_0017", "Nottingham City Council" },
        { "C_0018", "Oxfordshire County Council" },
        { "C_0019", "Portsmouth City Council" },
        { "C_0020", "Shropshire County Council" },
        { "C_0021", "Stroud District Council" },
        { "C_0022", "Suffolk County Council" },
        { "C_0023", "Surrey County Council" },
        { "C_0024", "Watford Borough Council" },
        { "C_0025", "West Devon Borough Council" },
        { "C_0026", "West Midlands Combined Authority" },
        { "C_0027", "Westmorland and Furness Council" },
    };
    
    public static readonly Dictionary<string, List<string>> ConsortiumCustodianCodesIdsByConsortiumCode = 
        BuildConsortiumCustodianCodesIdsByConsortiumCode();

    public static Dictionary<string, List<string>> BuildConsortiumCustodianCodesIdsByConsortiumCode()
    {
        return LocalAuthorityData.LocalAuthorityConsortiumCodeByCustodianCode
            .GroupBy(laToConsortiumPair => laToConsortiumPair.Value)
            .ToDictionary(group => group.Key, group => group.Select(laToConsortiumPair => laToConsortiumPair.Key).ToList());
    }
}

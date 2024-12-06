namespace HerPortal.BusinessLogic.Models;

/// <summary>
///     LA-Consortium mapping data can be found both in <see cref="LocalAuthorityData"/>, and in the HUG2 Public Website codebase's LocalAuthorityData.cs.
///     <seealso href="https://github.com/UKGovernmentBEIS/desnz-home-energy-retrofit-beta/blob/develop/HerPublicWebsite.BusinessLogic/Models/LocalAuthorityData.cs">Link to HUG2 Public Website codebase's LocalAuthorityData.cs</seealso>
///     Note: Mappings in the above files are expected to be consistent between both repos
/// </summary>
public static class ConsortiumData
{
    /// If any Custodian/Consortium codes are changed, consider if remapping is required in <see cref="LocalAuthorityData"/>
    public static readonly Dictionary<string, string> ConsortiumNamesByConsortiumCode = new()
    {
        { "C_0002", "Blackpool Council" },
        { "C_0003", "Bristol City Council" },
        { "C_0004", "Broadland District Council" },
        { "C_0006", "Cambridge City Council" },
        { "C_0007", "Cambridgeshire and Peterborough Combined Authority" },
        { "C_0008", "Cheshire East Council" },
        { "C_0010", "Cornwall Council and Council of the Isles of Scilly" },
        { "C_0012", "Darlington Borough Council" },
        { "C_0013", "Dartford Borough Council" },
        { "C_0014", "Devon County Council" },
        { "C_0015", "Dorset Council" },
        { "C_0016", "Cumberland Council and Westmoreland and Furness" },
        { "C_0017", "Greater London Authority" },
        { "C_0021", "Lewes District Council" },
        { "C_0022", "Liverpool City Region Combined Authority" },
        { "C_0024", "Midlands Net Zero Hub" },
        { "C_0027", "North Yorkshire Council" },
        { "C_0029", "Oxfordshire County Council" },
        { "C_0031", "Portsmouth City Council" },
        { "C_0033", "Sedgemoor" }, // Legacy Consortium
        { "C_0037", "Stroud District Council" },
        { "C_0038", "Suffolk County Council" },
        { "C_0039", "Surrey County Council" },
        { "C_0044", "West Devon Borough Council" }
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

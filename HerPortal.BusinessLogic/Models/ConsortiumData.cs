namespace HerPortal.BusinessLogic.Models;

public static class ConsortiumData
{
    // The mapping from custodian code to name comes from the publicly available "Local custodian codes" download link
    // on https://www.ordnancesurvey.co.uk/business-government/tools-support/addressbase-support
    public static readonly Dictionary<string, string> ConsortiumNamesByConsortiumCode = new()
    {
        { "C_0002", "Blackpool" },
        { "C_0003", "Bristol" },
        { "C_0004", "Broadland" },
        { "C_0006", "Cambridge" },
        { "C_0007", "Cambridgeshire & Peterborough Combined Authority" },
        { "C_0008", "Cheshire East" },
        { "C_0010", "Cornwall" },
        { "C_0012", "Darlington Borough Council" },
        { "C_0013", "Dartford" },
        { "C_0014", "Devon" },
        { "C_0015", "Dorset" },
        { "C_0016", "Eden District Council" },
        { "C_0017", "Greater London Authority" },
        { "C_0021", "Lewes" },
        { "C_0022", "Liverpool City Region" },
        { "C_0024", "Midlands NetZero Hub" },
        { "C_0027", "North Yorkshire County Council" },
        { "C_0029", "Oxfordshire County Council" },
        { "C_0031", "Portsmouth" },
        { "C_0033", "Sedgemoor" },
        { "C_0037", "Stroud" },
        { "C_0038", "Suffolk County Council" },
        { "C_0039", "Surrey County Council" },
        { "C_0044", "West Devon" }
    };
    
    public static readonly Dictionary<string, List<string>> ConsortiumCustodianCodesIdsByConsortiumCode = new()
    {
        { "C_0002", new List<string> { "2372", "2373", "2315", "2320", "2330", "2335", "2340", "2345", "2350", "2355", "2360", "2365", "2370" } },
        { "C_0003", new List<string> { "114", "116", "121" } },
        { "C_0004", new List<string> { "2605", "2610", "2635", "2620", "2625", "2630" } },
        { "C_0006", new List<string> { "505", "510", "515", "520", "530" } },
        { "C_0007", new List<string> { "2205", "1505", "235", "335", "1510", "1515", "1905", "440", "2210", "1520", "1525", "1530", "1910", "1915", "1535", "2250", "2230", "1540", "1730", "5480", "1920", "2235", "1545", "2280", "435", "1925", "2840", "3110", "345", "1550", "350", "1590", "1930", "1935", "5870", "2255", "1560", "2260", "1595", "2265", "2270", "1570", "1950", "340", "2845", "355", "360" } },
        { "C_0008", new List<string> { "660", "665" } },
        { "C_0010", new List<string> { "840", "835" } },
        { "C_0012", new List<string> { "1350", "724", "728", "738" } },
        { "C_0013", new List<string> { "2215", "2220" } },
        { "C_0014", new List<string> { "1105", "1110", "1135", "1115", "1130", "1165", "1145" } },
        { "C_0015", new List<string> { "1260", "1265" } },
        { "C_0016", new List<string> { "940", "935" } },
        { "C_0017", new List<string> { "5060", "5090", "5120", "5150", "5180", "5210", "5030", "5240", "5270", "5300", "5330", "5360", "5390", "5420", "5450", "5510", "5540", "5570", "5600", "2004", "5660", "5690", "5720", "5750", "5780", "5810", "5840", "5900", "5930", "5960", "5990" } },
        { "C_0021", new List<string> { "1410", "1415", "1425", "1430" } },
        { "C_0022", new List<string> { "650", "4305", "4310", "4320", "4315" } },
        { "C_0024", new List<string> { "1005", "3005", "3010", "4605", "2405", "2505", "1805", "3015", "3405", "2410", "1015", "4610", "1055", "1045", "4615", "2510", "3410", "1025", "3020", "2415", "1850", "1030", "2420", "3415", "2515", "1820", "3025", "2430", "3030", "3420", "2002", "2520", "2003", "3705", "2435", "3060", "3710", "2440", "1825", "3715", "3040", "4620", "3245", "4625", "1040", "2525", "2530", "3430", "3425", "3435", "3720", "3445", "3240", "4630", "3725", "2535", "4635", "1835", "1840", "1845" } },
        { "C_0027", new List<string> { "2705", "2710", "2715", "2745", "2720", "2725", "2730", "2735" } },
        { "C_0029", new List<string> { "3105", "3115", "3120", "3125" } },
        { "C_0031", new List<string> { "3805", "3810", "1705", "1445", "3815", "3820", "5240", "1710", "1715", "1720", "1725", "1735", "3825", "2114", "3830", "1740", "540", "1775", "1750", "2470", "1780", "1760", "1765", "3835" } },
        { "C_0033", new List<string> { "3305", "3310", "3330", "3325" } },
        { "C_0037", new List<string> { "1605", "1610", "1615", "1620", "119", "1625", "1625", "1630" } },
        { "C_0038", new List<string> { "3505", "3540", "3515", "3520", "3545" } },
        { "C_0039", new List<string> { "3605", "3610", "3615", "3620", "3625", "3630", "3635", "3640", "3645", "3650", "3655" } },
        { "C_0044", new List<string> { "1125", "1150" } }
    };
}

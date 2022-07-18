using HappyTravel.JuniperConnector.Data.Models;

namespace HappyTravel.JuniperConnector.Common.Infrastructure;

public static class ZoneServiceExtensions
{
    public static (string, string) GetCountryAndLocality(string zoneCode, List<Zone> zones)
    {
        Zone zone;
        var zoneHierarchy = new List<Zone>();

        do
        {
            zone = zones.Single(x => x.Code == zoneCode);
            zoneHierarchy.Add(zone);
            zoneCode = zone.ParentCode;
        } while (zone.ParentCode != null);

        var countryZone = zoneHierarchy.FirstOrDefault(x => CountryAreaTypes.Contains(x.AreaType));
        var country = countryZone is not null ? countryZone.Name : string.Empty;

        var localityZone = zoneHierarchy.FirstOrDefault(x => LocalityAreaTypes.Contains(x.AreaType));
        var locality = localityZone is not null ? localityZone.Name : string.Empty;

        return (country, locality);
    }


    private static readonly IReadOnlyCollection<ZoneType> CountryAreaTypes = new ZoneType[] { ZoneType.PAS, ZoneType.COL };
    private static readonly IReadOnlyCollection<ZoneType> LocalityAreaTypes = new ZoneType[] { ZoneType.CTY, ZoneType.LOC, ZoneType.REG };
}

using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Common.JuniperService;

public interface IJuniperServiceClient
{
    public Task<List<JP_Zone>> GetZoneList();
}

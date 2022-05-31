using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Common.JuniperService;

public interface IJuniperServiceClient
{
    public Task<List<JP_Zone>> GetZoneList();    
    public Task<List<JP_HotelContent>> GetHotelContents(IEnumerable<string> hotelCodes);
    public Task<JP_HotelPortfolio> GetHotelPortfolio(int recordsPerPage, string nextToken);
}

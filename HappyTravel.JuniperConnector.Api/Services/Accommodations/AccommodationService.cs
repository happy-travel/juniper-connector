using HappyTravel.BaseConnector.Api.Services.Accommodations;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.JuniperConnector.Api.Services.Accommodations;

public class AccommodationService : IAccommodationService
{
    public Task<List<MultilingualAccommodation>> Get(int skip, int top, DateTimeOffset? modificationDate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

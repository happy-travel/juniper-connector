using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.AccommodationAvailabilities;

public class AccommodationAvailabilityService : IAccommodationAvailabilityService
{
    public Task<Result<AccommodationAvailability>> Get(string availabilityId, string accommodationId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

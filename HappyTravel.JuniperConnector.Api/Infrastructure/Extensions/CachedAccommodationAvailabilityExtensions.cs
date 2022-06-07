using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Models.Availability;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class CachedAccommodationAvailabilityExtensions
{
    public static SlimAccommodationAvailability ToContract(this CachedAccommodationAvailability cachedAccommodationAvailability, string availabilityId)
        => new(accommodationId: cachedAccommodationAvailability.AccommodationId.ToString(),
            roomContractSets: cachedAccommodationAvailability.CachedRoomContractSets
                .Select(s => s.ToContract())
                .ToList(),
            availabilityId: availabilityId);
}

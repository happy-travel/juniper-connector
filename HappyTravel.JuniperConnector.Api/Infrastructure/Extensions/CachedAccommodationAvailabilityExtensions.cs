using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Models.Availability;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class CachedAccommodationAvailabilityExtensions
{
    /// <summary>
    /// Converts an object of type CachedAccommodationAvailability to SlimAccommodationAvailability
    /// </summary>
    /// <param name="cachedAccommodationAvailability">Cached accommodationa vailability object</param>
    /// <param name="availabilityId">Availability id</param>
    /// <returns></returns>
    public static SlimAccommodationAvailability ToContract(this CachedAccommodationAvailability cachedAccommodationAvailability, string availabilityId)
        => new(accommodationId: cachedAccommodationAvailability.AccommodationId.ToString(),
            roomContractSets: cachedAccommodationAvailability.CachedRoomContractSets
                .Select(s => s.ToContract())
                .ToList(),
            availabilityId: availabilityId);
}

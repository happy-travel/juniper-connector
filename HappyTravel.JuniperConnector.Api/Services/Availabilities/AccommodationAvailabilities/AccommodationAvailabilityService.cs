using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Models.Availability;
using HappyTravel.JuniperConnector.Api.Services.Caching;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.AccommodationAvailabilities;

public class AccommodationAvailabilityService : IAccommodationAvailabilityService
{
    public AccommodationAvailabilityService(AvailabilityRequestStorage requestStorage,
        AvailabilitySearchResultStorage availabilitySearchResultStorage)
    {
        _requestStorage = requestStorage;
        _availabilitySearchResultStorage = availabilitySearchResultStorage;
    }


    public async Task<Result<AccommodationAvailability>> Get(string availabilityId, string accommodationId, CancellationToken cancellationToken)
    {
        return await GetRequest()
            .Bind(GetCachedAccommodationAvailability)
            .Map(MapToContracts);


        Task<Result<AvailabilityRequest>> GetRequest()
            => _requestStorage.Get(availabilityId);


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability)>> GetCachedAccommodationAvailability(AvailabilityRequest request)
        {
            var (isSuccess, _, accommodationAvailability, error) = await _availabilitySearchResultStorage.GetByAccommodationId(availabilityId, accommodationId);

            if (isSuccess)
                return (request, accommodationAvailability);

            return Result.Failure<(AvailabilityRequest, CachedAccommodationAvailability)>(error);
        }


        AccommodationAvailability MapToContracts((AvailabilityRequest, CachedAccommodationAvailability) result)
        {
            var (request, accommodationAvailability) = result;

            return new AccommodationAvailability(availabilityId: availabilityId,
                accommodationId: accommodationAvailability.AccommodationId,
                checkInDate: request.CheckInDate,
                checkOutDate: request.CheckOutDate,
                numberOfNights: request.GetNumberOfNights(),
                roomContractSets: accommodationAvailability.CachedRoomContractSets
                    .Select(s => s.ToContract())
                    .ToList());
        }
    }


    private readonly AvailabilityRequestStorage _requestStorage;
    private readonly AvailabilitySearchResultStorage _availabilitySearchResultStorage;
}

using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Infrastructure;
using HappyTravel.BaseConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Models.Availability;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.AccommodationAvailabilities;

public class AccommodationAvailabilityService : IAccommodationAvailabilityService
{
    public AccommodationAvailabilityService(AvailabilityRequestStorage requestStorage,
        AvailabilitySearchResultStorage availabilitySearchResultStorage)
    {
        _requestStorage = requestStorage;
        _availabilitySearchResultStorage = availabilitySearchResultStorage;
    }


    /// <summary>
    /// Searches for specific accommodation with available room contract sets. The 2nd search step. 
    /// </summary>
    /// <param name="accommodationId">Supplier accommodation id</param>
    /// <param name="availabilityId">Availability Id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public async Task<Result<AccommodationAvailability, ProblemDetails>> Get(string availabilityId, string accommodationId, CancellationToken cancellationToken)
    {
        return await GetRequest()
            .Bind(GetCachedAccommodationAvailability)
            .Map(MapToContracts);


        async Task<Result<AvailabilityRequest, ProblemDetails>> GetRequest()
        {
            var (_, isFailure, request, error) = await _requestStorage.Get(availabilityId);
            if (isFailure)
                return ProblemDetailsBuilder.CreateFailureResult<AvailabilityRequest>(error, SearchFailureCodes.Unknown);

            return request;
        }        


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability), ProblemDetails>> GetCachedAccommodationAvailability(AvailabilityRequest request)
        {
            var (isSuccess, _, accommodationAvailability, error) = await _availabilitySearchResultStorage.GetByAccommodationId(availabilityId, accommodationId);

            if (isSuccess)
                return (request, accommodationAvailability);

            return ProblemDetailsBuilder.CreateFailureResult<(AvailabilityRequest, CachedAccommodationAvailability)>(error, SearchFailureCodes.AvailabilityNotFound);
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

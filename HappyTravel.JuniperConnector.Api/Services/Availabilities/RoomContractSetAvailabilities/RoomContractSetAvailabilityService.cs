using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.RoomContractSetAvailabilities;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Models.Availability;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.RoomContractSetAvailabilities;

public class RoomContractSetAvailabilityService : IRoomContractSetAvailabilityService
{
    public RoomContractSetAvailabilityService(JuniperClient client,
        AvailabilityRequestStorage requestStorage,
        AvailabilitySearchResultStorage availabilitySearchResultStorage,
        BookingCodeStorage bookingCodeStorage)
    {
        _client = client;
        _requestStorage = requestStorage;
        _availabilitySearchResultStorage = availabilitySearchResultStorage;
        _bookingCodeStorage = bookingCodeStorage;
    }


    public async Task<Result<RoomContractSetAvailability?>> Get(string availabilityId, Guid roomContractSetId, CancellationToken cancellationToken)
    {
        return await GetRequest()
            .Bind(GetSearchResult)
            .Bind(GetHotelbookingRules)
            .Map(ToContracts);


        Task<Result<AvailabilityRequest>> GetRequest()
            => _requestStorage.Get(availabilityId);


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability)>> GetSearchResult(AvailabilityRequest availabilityRequest)
        {
            var (isSuccess, _, searchResult, error) = await _availabilitySearchResultStorage.GetByRoomContractSetId(availabilityId, roomContractSetId);

            if (isSuccess)
                return (availabilityRequest, searchResult);

            return Result.Failure<(AvailabilityRequest, CachedAccommodationAvailability)>(error);
        }


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability, JP_BookingRules)>> GetHotelbookingRules((AvailabilityRequest, CachedAccommodationAvailability) cachedData)
        {
            var (availabilityRequest, accommodationAvailability) = cachedData;
            var ratePlanCode = accommodationAvailability.CachedRoomContractSets.Single(r => r.Id == roomContractSetId).RatePlanCode;

            var (isSuccess, _, response, error) = await _client.GetHotelbookingRules(availabilityRequest.ToJuniperBookingRulesRequest(ratePlanCode, accommodationAvailability.AccommodationId));

            if (isSuccess)
                return (availabilityRequest, accommodationAvailability, response);

            return Result.Failure<(AvailabilityRequest, CachedAccommodationAvailability, JP_BookingRules)>(error);
        }


        async Task<RoomContractSetAvailability?> ToContracts((AvailabilityRequest, CachedAccommodationAvailability, JP_BookingRules) data)
        {
            var (availabilityRequest, accommodationAvailability, bookingRules) = data;

            var cachedRoomContractSet = accommodationAvailability.CachedRoomContractSets.Single(r => r.Id == roomContractSetId);

            var hotelResults = bookingRules.Items.Select(hotelResult => (JP_HotelResultsBookingRules)hotelResult).ToList();
            var hotelOption = hotelResults.Single().HotelOptions.Single();

            await _bookingCodeStorage.Set(availabilityId, cachedRoomContractSet.Id, hotelOption.BookingCode.Value);

            var numberOfNights = availabilityRequest.GetNumberOfNights();
            var roomContractSet = cachedRoomContractSet.ToContract().Update(hotelOption.CancellationPolicy, 
                hotelOption.PriceInformation.Prices.Single(), 
                availabilityRequest.CheckInDate, 
                numberOfNights, 
                hotelOption.OptionalElements.Comments);

            return new RoomContractSetAvailability(availabilityId: availabilityId,
                accommodationId: accommodationAvailability.AccommodationId,
                checkInDate: availabilityRequest.CheckInDate,
                checkOutDate: availabilityRequest.CheckOutDate,
                numberOfNights: numberOfNights,
                roomContractSet: roomContractSet,
                creditCardRequirement: GetCreditCardRequirement());


            CreditCardRequirement? GetCreditCardRequirement()
                => new(roomContractSet.Deadline.Policies.Min(p => p.FromDate), availabilityRequest.CheckOutDate);
        }
    }


    private readonly JuniperClient _client;
    private readonly AvailabilityRequestStorage _requestStorage;
    private readonly AvailabilitySearchResultStorage _availabilitySearchResultStorage;
    private readonly BookingCodeStorage _bookingCodeStorage;
}

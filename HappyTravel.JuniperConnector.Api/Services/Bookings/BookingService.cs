using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Infrastructure;
using HappyTravel.BaseConnector.Api.Services.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Models.Availability;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using JuniperServiceReference;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.JuniperConnector.Api.Services.Bookings;

public class BookingService : IBookingService
{
    public BookingService(JuniperClient juniperClient, AvailabilityRequestStorage availabilityRequestStorage,
        AvailabilitySearchResultStorage availabilitySearchResultStorage,
        BookingCodeStorage bookingCodeStorage,
        BookingManager bookingManager)
    {
        _juniperClient = juniperClient;
        _availabilityRequestStorage = availabilityRequestStorage;
        _availabilitySearchResultStorage = availabilitySearchResultStorage;
        _bookingCodeStorage = bookingCodeStorage;
        _bookingManager = bookingManager;
    }


    public async Task<Result<Booking, ProblemDetails>> Book(BookingRequest bookingRequest, CancellationToken cancellationToken)
    {
        return await GetCachedData()
            .Bind(Book)
            .Tap(SaveBooking)
            .Map(MapToContract);


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability, string), ProblemDetails>> GetCachedData()
        {
            var availabilityRequest = await _availabilityRequestStorage.Get(bookingRequest.AvailabilityId);
            if (availabilityRequest.IsFailure)
                return ProblemDetailsBuilder.CreateFailureResult<(AvailabilityRequest, CachedAccommodationAvailability, string)>(availabilityRequest.Error, BookingFailureCodes.ConnectorValidationFailed);

            var accommodationAvailability = await _availabilitySearchResultStorage.GetByRoomContractSetId(bookingRequest.AvailabilityId, bookingRequest.RoomContractSetId);
            if (accommodationAvailability.IsFailure)
                return ProblemDetailsBuilder.CreateFailureResult<(AvailabilityRequest, CachedAccommodationAvailability, string)>(accommodationAvailability.Error, BookingFailureCodes.ConnectorValidationFailed);

            var bookingCode = await _bookingCodeStorage.Get(bookingRequest.AvailabilityId, bookingRequest.RoomContractSetId);
            if (bookingCode.IsFailure)
                return ProblemDetailsBuilder.CreateFailureResult<(AvailabilityRequest, CachedAccommodationAvailability, string)>(bookingCode.Error, BookingFailureCodes.ConnectorValidationFailed);

            return (availabilityRequest.Value, accommodationAvailability.Value, bookingCode.Value);
        }


        async Task<Result<(AvailabilityRequest, CachedAccommodationAvailability, JP_Reservation), ProblemDetails>> Book((AvailabilityRequest, CachedAccommodationAvailability, string) cachedData)
        {
            var (availabilityRequest, cachedAccommodationAvailability, bookingCode) = cachedData;
            var cachedRoomContractSet = cachedAccommodationAvailability.CachedRoomContractSets.Single(r => r.Id == bookingRequest.RoomContractSetId);

            var (isSuccess, _, booking, error) = await _juniperClient.Book(bookingRequest.ToJuniperBookingRequest(bookingCode,
                cachedAccommodationAvailability.AccommodationId,
                availabilityRequest.CheckInDate,
                availabilityRequest.CheckOutDate,
                Convert.ToDouble(cachedRoomContractSet.Rate.FinalPrice.Amount),
                cachedRoomContractSet.Rate.Currency));

            if (isSuccess)
                return (availabilityRequest, cachedAccommodationAvailability, booking.Single());

            return ProblemDetailsBuilder.CreateFailureResult<(AvailabilityRequest, CachedAccommodationAvailability, JP_Reservation)>(error, BookingFailureCodes.RequestException);
        }


        async Task SaveBooking((AvailabilityRequest, CachedAccommodationAvailability, JP_Reservation) result)
        {
            var(availabilityRequest, cachedAccommodationAvailability, reservation) = result;

            await _bookingManager.Add(new Data.Models.Booking()
            {
                ReferenceCode = bookingRequest.ReferenceCode,
                SupplierReferenceCode = reservation.Locator,
                CheckInDate = availabilityRequest.CheckInDate,
                CheckOutDate = availabilityRequest.CheckOutDate
            });
        }


        Booking MapToContract((AvailabilityRequest, CachedAccommodationAvailability, JP_Reservation) result)
        {
            var (availabilityRequest, cachedAccommodationAvailability, reservation) = result;

            return BookingMapper.Map(reservation, bookingRequest.ReferenceCode, availabilityRequest.CheckInDate, availabilityRequest.CheckOutDate);
        }
    }


    public async Task<Result> Cancel(string referenceCode, CancellationToken cancellationToken)
    {
        return await _bookingManager.Get(referenceCode)
            .Bind(CancelBooking);


        async Task<Result> CancelBooking(Data.Models.Booking booking)
        {
            var request = new JP_CancelRQ()
            {
                CancelRequest = new JP_CancelRequest()
                {
                    ReservationLocator = booking.SupplierReferenceCode
                }
            };

            var (isSuccess, _, response, error) = await _juniperClient.CancelBooking(request);

            if (isSuccess)
            {
                var reservation = response.Single();

                if (reservation.Status == JP_ResStatus.CAN || reservation.Status == JP_ResStatus.CAC)
                    return Result.Success();
            }

            return Result.Failure(error);
        }
    }


    public Task<Result<Booking>> Get(string referenceCode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


    private readonly JuniperClient _juniperClient;
    private readonly AvailabilityRequestStorage _availabilityRequestStorage;
    private readonly AvailabilitySearchResultStorage _availabilitySearchResultStorage;
    private readonly BookingCodeStorage _bookingCodeStorage;
    private readonly BookingManager _bookingManager;
}

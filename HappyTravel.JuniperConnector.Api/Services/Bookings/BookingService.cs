using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.JuniperConnector.Api.Services.Bookings;

public class BookingService : IBookingService
{
    public Task<Result<Booking, ProblemDetails>> Book(BookingRequest bookingRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result> Cancel(string referenceCode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Booking>> Get(string referenceCode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

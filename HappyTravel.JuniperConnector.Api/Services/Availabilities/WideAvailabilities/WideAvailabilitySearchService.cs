using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.WideAvailabilities;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Data.Models;
using JuniperServiceReference;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.WideAvailabilities;

public class WideAvailabilitySearchService : IWideAvailabilitySearchService
{
    public WideAvailabilitySearchService(JuniperContext context,
        AvailabilityRequestStorage requestStorage,
        AvailabilitySearchMapper availabilitySearchMapper,
        WideAvailabilitySearchRequestExecutor requestExecutor)
    {
        _context = context;
        _requestStorage = requestStorage;
        _availabilitySearchMapper = availabilitySearchMapper;
        _requestExecutor = requestExecutor;
    }


    public async Task<Result<Availability>> Get(AvailabilityRequest request, string languageCode, CancellationToken cancellationToken)
    {
        //var accommodations = await GetAccommodations();
        var accommodations = new Dictionary<string, Accommodation>() { { "JP046300", new Accommodation() } };

        if (!accommodations.Any())
            return new Availability(string.Empty, request.GetNumberOfNights(),
                request.CheckInDate, request.CheckOutDate,
                new List<SlimAccommodationAvailability>(0));

        return await GetHotelResults()
            .Map(ToContracts)
            .Tap(SaveRequest);


        async Task<Dictionary<string, Accommodation>> GetAccommodations()
        {
            var accommodationCodes = request.AccommodationIds.ToHashSet();
            return await _context.Accommodations
                .Where(acc => accommodationCodes.Contains(acc.Code))
                .ToDictionaryAsync(acc => acc.Code, acc => acc);
        }


        Task<Result<List<JP_Results>>> GetHotelResults()
            => _requestExecutor.GetHotelResults(request, cancellationToken);


        Task<Availability> ToContracts(List<JP_Results> responses)
            => _availabilitySearchMapper.MapToAvailability(request, responses);


        Task SaveRequest(Availability availability)
             => _requestStorage.Set(availability.AvailabilityId, request);
    }


    private readonly JuniperContext _context;
    private readonly AvailabilityRequestStorage _requestStorage;
    private readonly AvailabilitySearchMapper _availabilitySearchMapper;
    private readonly WideAvailabilitySearchRequestExecutor _requestExecutor;
}

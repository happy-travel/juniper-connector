using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.JuniperConnector.Api.Infrastructure.Logging;

namespace HappyTravel.JuniperConnector.Api.Services.Caching;

public class BookingCodeStorage
{
    public BookingCodeStorage(IDoubleFlow flow, ILogger<BookingCodeStorage> logger)
    {
        _flow = flow;
        _logger = logger;
    }


    public async Task<Result<string>> Get(string availabilityId, Guid roomContractSetId)
    {
        var data = await _flow.GetAsync<string>(BuildKey(availabilityId, roomContractSetId), RequestCacheLifeTime);

        if (!string.IsNullOrWhiteSpace(data))
            return data;

        _logger.LogGetBookingCodeFromStorageFailed(availabilityId, roomContractSetId);
        return Result.Failure<string>($"Could not get bookingCode");
    }


    public Task Set(string availabilityId, Guid roomContractSetId, string bookingCode)
        => _flow.SetAsync(BuildKey(availabilityId, roomContractSetId), bookingCode, RequestCacheLifeTime);


    private string BuildKey(string availabilityId, Guid roomContractSetId)
        => _flow.BuildKey(nameof(BookingCodeStorage), availabilityId, roomContractSetId.ToString());


    private static TimeSpan RequestCacheLifeTime => Constants.StepCacheLifeTime * 2; // The BookingCode lifetime is 10 minutes. Defined in the API documentation.

    private readonly IDoubleFlow _flow;
    private readonly ILogger<BookingCodeStorage> _logger;
}

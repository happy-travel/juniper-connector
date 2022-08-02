using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.JuniperConnector.Api.Infrastructure.Logging;

namespace HappyTravel.JuniperConnector.Api.Services.Caching;

public class AvailabilityRequestStorage
{
    public AvailabilityRequestStorage(IDoubleFlow flow, ILogger<AvailabilityRequestStorage> logger)
    {
        _flow = flow;
        _logger = logger;
    }


    /// <summary>
    /// Retrieving availability request from cache
    /// </summary>
    /// <param name="availabilityId">Availability id</param>
    /// <returns></returns>
    public async Task<Result<AvailabilityRequest>> Get(string availabilityId)
    {
        var availabilityRequest = await _flow.GetAsync<AvailabilityRequest?>(BuildKey(availabilityId), RequestCacheLifeTime);

        if (availabilityRequest is not null)
            return availabilityRequest.Value;

        _logger.LogGetAvailabilityRequestFromStorageFailed(availabilityId);
        return Result.Failure<AvailabilityRequest>($"Could not get availability with id {availabilityId}");
    }


    /// <summary>
    /// Save availability request to cache
    /// </summary>
    /// <param name="availabilityId">Availability id</param>
    /// <param name="request">Availability request</param>
    /// <returns></returns>
    public Task Set(string availabilityId, AvailabilityRequest request)
        => _flow.SetAsync(BuildKey(availabilityId), request, RequestCacheLifeTime);


    private string BuildKey(string availabilityId)
        => _flow.BuildKey(nameof(AvailabilityRequestStorage), availabilityId);


    private static TimeSpan RequestCacheLifeTime => Constants.StepCacheLifeTime * 5;

    private readonly IDoubleFlow _flow;
    private readonly ILogger<AvailabilityRequestStorage> _logger;
}

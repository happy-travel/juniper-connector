using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.JuniperConnector.Api.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Api.Models.Availability;

namespace HappyTravel.JuniperConnector.Api.Services.Caching;

public class AvailabilitySearchResultStorage
{
    public AvailabilitySearchResultStorage(IDoubleFlow flow, ILogger<AvailabilitySearchResultStorage> logger)
    {
        _flow = flow;
        _logger = logger;
    }


    /// <summary>
    /// Retrieving CachedAccommodationAvailability from the cache by accommodation id
    /// </summary>
    /// <param name="availabilityId">Availability id</param>
    /// <param name="accommodationId">Accommodation id</param>
    /// <returns></returns>
    public async Task<Result<CachedAccommodationAvailability>> GetByAccommodationId(string availabilityId, string accommodationId)
      => await GetCachedData(BuildKey(availabilityId), a => a.AccommodationId == accommodationId);


    /// <summary>
    /// Retrieving CachedAccommodationAvailability from the cache by room contract set id
    /// </summary>
    /// <param name="availabilityId">Availability id</param>
    /// <param name="roomContractSetId">Room contract set id</param>
    /// <returns></returns>
    public async Task<Result<CachedAccommodationAvailability>> GetByRoomContractSetId(string availabilityId, Guid roomContractSetId)
        => await GetCachedData(BuildKey(availabilityId), a => a.CachedRoomContractSets
            .Any(r => r.Id == roomContractSetId));


    /// <summary>
    /// Caching a list of CachedAccommodationAvailability
    /// </summary>
    /// <param name="availabilityId">Availability id</param>
    /// <param name="data">List of CachedAccommodationAvailability</param>
    /// <returns></returns>
    public Task Set(string availabilityId, List<CachedAccommodationAvailability> data)
        => _flow.SetAsync(BuildKey(availabilityId), data, RequestCacheLifeTime);


    private string BuildKey(string availabilityId)
        => _flow.BuildKey(nameof(AvailabilitySearchResultStorage), availabilityId);


    private async Task<Result<CachedAccommodationAvailability>> GetCachedData(string key, Func<CachedAccommodationAvailability, bool> filter)
    {
        var data = await _flow.GetAsync<List<CachedAccommodationAvailability>>(key, RequestCacheLifeTime);
        var accommodationAvailability = data?.SingleOrDefault(filter);

        if (accommodationAvailability is not null)
            return accommodationAvailability;

        _logger.LogGetAccommodationFromStorageFailed(key);
        return Result.Failure<CachedAccommodationAvailability>("Could not get cached data");
    }


    private static TimeSpan RequestCacheLifeTime => Constants.StepCacheLifeTime * 2; 

    private readonly IDoubleFlow _flow;
    private readonly ILogger<AvailabilitySearchResultStorage> _logger;
}

using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;
using JuniperServiceReference;
using System.Collections.Concurrent;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.WideAvailabilities;

public class WideAvailabilitySearchRequestExecutor
{
    public WideAvailabilitySearchRequestExecutor(JuniperAvailTransactionsClient juniperClient)
    {
        _juniperClient = juniperClient;
    }


    public async Task<Result<List<JP_Results>>> GetHotelResults(AvailabilityRequest request, CancellationToken cancellationToken)
    {
        var accommodationIds = request.AccommodationIds;

        if (accommodationIds.Count <= MaxAllowedHotelCodes)
        {
            var (isSuccess, _, response, error) = await GetHotelResults(accommodationIds).Map(r => new List<JP_Results> { r });

            return isSuccess
                ? response
                : Result.Failure<List<JP_Results>>(error);
        }

        var responsesBag = new ConcurrentBag<JP_Results>();
        var errorsBag = new ConcurrentBag<string>();
        var accommodationCodeGroups = Split(accommodationIds, MaxAllowedHotelCodes).ToList();

        await accommodationCodeGroups
            .Select(GetHotelResults)
            .Select(AddToBag)
            .WhenAll();

        if (!errorsBag.IsEmpty && errorsBag.Count == accommodationCodeGroups.Count)
        {
            var errors = errorsBag.ToList();

            return Result.Failure<List<JP_Results>>(string.Join("; ", errors.Distinct()));
        }

        return responsesBag.ToList();

        Task<Result<JP_Results>> GetHotelResults(List<string> accommodationIds)
            => _juniperClient.GetHotelAvailability(request.ToJuniperAvailabilityRequest(accommodationIds));


        IEnumerable<List<T>> Split<T>(List<T> list, int size)
        {
            for (int i = 0; i < list.Count; i += size)
                yield return list.GetRange(i, Math.Min(size, list.Count - i));
        }


        async Task AddToBag(Task<Result<JP_Results>> result)
        {
            var (isSuccess, _, response, error) = await result;
            if (isSuccess)
                responsesBag.Add(response);
            else
                errorsBag.Add(error);
        }
    }


    private const int MaxAllowedHotelCodes = 500; // Due to limits: "max 2000 hotels per search"

    JuniperAvailTransactionsClient _juniperClient;
}

using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.WideAvailabilities;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.WideAvailabilities;

public class WideAvailabilitySearchService : IWideAvailabilitySearchService
{
    public Task<Result<Availability>> Get(AvailabilityRequest request, string languageCode, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

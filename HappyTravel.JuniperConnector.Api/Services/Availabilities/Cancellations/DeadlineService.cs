using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.Cancellations;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.Cancellations;

public class DeadlineService : IDeadlineService
{
    public Task<Result<Deadline>> Get(string availabilityId, Guid roomContractSetId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

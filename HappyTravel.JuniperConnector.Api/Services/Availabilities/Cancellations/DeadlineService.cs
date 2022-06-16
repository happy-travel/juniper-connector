using CSharpFunctionalExtensions;
using HappyTravel.BaseConnector.Api.Services.Availabilities.Cancellations;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.JuniperConnector.Api.Services.Availabilities.Cancellations;

public class DeadlineService : IDeadlineService
{
    public Task<Result<Deadline>> Get(string availabilityId, Guid roomContractSetId, CancellationToken cancellationToken)
        => Task.FromResult(Result.Failure<Deadline>("Our current implementation and settings shows deadlines on the first two steps"));
}

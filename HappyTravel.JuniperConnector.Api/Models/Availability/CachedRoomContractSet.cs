using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;

namespace HappyTravel.JuniperConnector.Api.Models.Availability;

public record CachedRoomContractSet(string RatePlanCode,
    Guid Id,
    in Rate Rate,
    Deadline Deadline,
    List<RoomContract> Rooms,
    List<string> Tags,
    bool IsDirectContract,
    bool IsAdvancePurchaseRate,
    bool IsPackageRate);

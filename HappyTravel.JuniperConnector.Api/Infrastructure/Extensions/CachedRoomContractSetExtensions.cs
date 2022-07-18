using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Models.Availability;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class CachedRoomContractSetExtensions
{
    public static RoomContractSet ToContract(this CachedRoomContractSet roomContractSet)
        => new(roomContractSet.Id,
            roomContractSet.Rate,
            roomContractSet.Deadline,
            roomContractSet.Rooms,
            roomContractSet.Tags,
            roomContractSet.IsDirectContract,
            roomContractSet.IsAdvancePurchaseRate,
            roomContractSet.IsPackageRate);
}

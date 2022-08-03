using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.JuniperConnector.Api.Models.Availability;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class CachedRoomContractSetExtensions
{
    /// <summary>
    /// Converts an object of type CachedRoomContractSet to type RoomContractSet
    /// </summary>
    /// <param name="roomContractSet">Cached room contract set</param>
    /// <returns></returns>
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

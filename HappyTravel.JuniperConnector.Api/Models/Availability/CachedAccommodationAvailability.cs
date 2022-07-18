namespace HappyTravel.JuniperConnector.Api.Models.Availability;

public record CachedAccommodationAvailability(string AccommodationId, List<CachedRoomContractSet> CachedRoomContractSets);
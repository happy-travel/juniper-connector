using HappyTravel.EdoContracts.GeoData;
using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.Geography;
using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Data.Models;

namespace HappyTravel.JuniperConnector.Api.Services.Locations;

public class LocationMapper
{
    public LocationMapper(JuniperSerializer serializer)
    {
        _serializer = serializer;
    }


    public Location MapAccommodationToLocation(Accommodation accommodation)
    {
        return new Location(name: GetDefaultLocalizedName(accommodation.Name),
            locality: GetDefaultLocalizedName(accommodation.Locality ?? string.Empty),
            country: GetDefaultLocalizedName(accommodation.Country),
            coordinates: new GeoPoint(accommodation.Coordinates),
            distance: default,
            source: PredictionSources.Interior,
            type: LocationTypes.Accommodation);
    }


    public Location MapLocalityToLocation(string country, string locality)
    {
        return new Location(name: string.Empty,
            locality: GetDefaultLocalizedName(locality),
            country: GetDefaultLocalizedName(country),
            coordinates: default,
            distance: default,
            source: PredictionSources.Interior,
            type: LocationTypes.Location);
    }


    private string GetDefaultLocalizedName(string name)
    {
        var localizedName = new Dictionary<string, string> { { Constants.DefaultLanguageCode, name } };
        return _serializer.Serialize(localizedName);
    }


    private readonly JuniperSerializer _serializer;
}

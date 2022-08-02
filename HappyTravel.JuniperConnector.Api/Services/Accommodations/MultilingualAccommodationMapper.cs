using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Geography;
using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Common.Infrastructure;
using HappyTravel.JuniperConnector.Data.Models;
using HappyTravel.MultiLanguage;
using JuniperServiceReference;

namespace HappyTravel.JuniperConnector.Api.Services.Accommodations;

public class MultilingualAccommodationMapper
{
    public MultilingualAccommodationMapper(JuniperSerializer serializer)
    {
        _serializer = serializer;
    }


    public MultilingualAccommodation Map(Hotel hotel, List<Zone> zones)
    {
        var deserializedData = _serializer.Deserialize<JP_HotelContent>(hotel.Data);

        return new MultilingualAccommodation(
            supplierCode: hotel.Code,
            name: GetMultiLingualName(deserializedData),
            accommodationAmenities: new MultiLanguage<List<string>>(),
            additionalInfo: GetMultiLingualAdditionalInfo(deserializedData),
            category: GetMultilingualCategory(deserializedData),
            contacts: GetContacts(deserializedData),
            location: GetMultilingualLocationInfo(deserializedData, zones),
            photos: GetImages(deserializedData),
            rating: GetRating(deserializedData),
            schedule: GetSchedule(deserializedData),
            textualDescriptions: GetMultiLingualDescription(deserializedData),
            type: PropertyTypes.Hotels,
            isActive: hotel.IsActive,
            uniqueCodes: GetUniqueCodes(deserializedData)
        );
    }


    private MultiLanguage<string> GetMultiLingualName(JP_HotelContent hotelDetails)
    {
        var multilingualName = new MultiLanguage<string>();

        multilingualName.TrySetValue(Constants.DefaultLanguageCode, hotelDetails.HotelName);

        return multilingualName;
    }


    private MultiLanguage<Dictionary<string, string>> GetMultiLingualAdditionalInfo(JP_HotelContent hotelDetails)
    {
        var multilingualAdditionalInfo = new MultiLanguage<Dictionary<string, string>>();

        if (hotelDetails.Features is not null)
            multilingualAdditionalInfo.TrySetValue(Constants.DefaultLanguageCode,
                new Dictionary<string, string> { { "Features", string.Join(";", hotelDetails.Features.Select(x => x.Value1)) } });

        return multilingualAdditionalInfo;
    }


    private MultiLanguage<string> GetMultilingualCategory(JP_HotelContent hotelDetails)
    {        
        var multilingualCategory = new MultiLanguage<string>();

        var hotelCategory = hotelDetails.HotelCategory?.Value;
        if (hotelCategory is not null)
            multilingualCategory.TrySetValue(Constants.DefaultLanguageCode, hotelCategory);

        return multilingualCategory;
    }


    private ContactInfo GetContacts(JP_HotelContent hotelDetails)
    {
        return new ContactInfo(
            emails: hotelDetails.ContactInfo?.Emails?.Select(x => x.Value).ToList(),
            phones: hotelDetails.ContactInfo?.PhoneNumbers?.Select(x => x.Value).ToList(),
            webSites: new List<string>(),
            faxes: hotelDetails.ContactInfo?.Faxes?.Select(x => x.Value).ToList()
        );
    }


    private static AccommodationRatings GetRating(JP_HotelContent hotelDetails)
    {
        var hotelCategoryType = hotelDetails.HotelCategory?.Type;

        return hotelCategoryType switch
        {
            "1est" => AccommodationRatings.OneStar,
            "2est" => AccommodationRatings.TwoStars,
            "3est" => AccommodationRatings.ThreeStars,
            "4est" => AccommodationRatings.FourStars,
            "5est" => AccommodationRatings.FiveStars,
            _ => AccommodationRatings.NotRated
        };
    }
    


    static List<ImageInfo> GetImages(JP_HotelContent data)
            => data?.Images?.Select(x => new ImageInfo(x.FileName, x.Title)).ToList();


    private MultilingualLocationInfo GetMultilingualLocationInfo(JP_HotelContent hotelDetails, List<Zone> zones)
    {        
        var (country, locality) = ZoneServiceExtensions.GetCountryAndLocality(hotelDetails.Zone.Code, zones);
        var multilingualLocalityName = new MultiLanguage<string>();

        multilingualLocalityName.TrySetValue(Constants.DefaultLanguageCode, locality);

        var multilingualAddress = new MultiLanguage<string>();
        var address = hotelDetails.Address?.Address;
        if (address is not null)
            multilingualAddress.TrySetValue(Constants.DefaultLanguageCode, hotelDetails.Address.Address);

        var multilingualCountry = new MultiLanguage<string>();

        multilingualCountry.TrySetValue(Constants.DefaultLanguageCode, country);

        var multilingualCountryCode = Constants.VisibleCultures.FirstOrDefault(x => x.Value == country).Key;        

        var coordinates = GetPoint(hotelDetails.Address?.Latitude, hotelDetails.Address?.Longitude);

        var multilingualZone = new MultiLanguage<string>();

        multilingualZone.TrySetValue(Constants.DefaultLanguageCode, hotelDetails.Zone.Name);

        return new MultilingualLocationInfo(
            countryCode: multilingualCountryCode,
            country: multilingualCountry,
            locality: multilingualLocalityName,
            localityZone: multilingualZone,
            coordinates: coordinates,
            address: multilingualAddress,
            locationDescriptionCode: LocationDescriptionCodes.Unspecified,
            pointsOfInterests: hotelDetails.PointsOfInterest?.Select(x => new PoiInfo(x.Name, GetPointDistance(x.Distance), default, PoiTypes.PointOfInterest)).ToList());


        static double GetPointDistance(string distanceString)
            => double.TryParse(distanceString.Split(" ").First().Replace('.', ','), out double distance)
            ? distance
            : default;        
    }


    private static GeoPoint GetPoint(string? latitudeString, string? longitudeString)
    {
        if (latitudeString is null || longitudeString is null)
            return new GeoPoint(0, 0);

        return double.TryParse(latitudeString.Replace('.', ','), out double latitude) && double.TryParse(longitudeString.Replace('.', ','), out double longitude)
            ? new GeoPoint(latitude, longitude)
            : new GeoPoint(0, 0);
    }
       


    private static ScheduleInfo GetSchedule(JP_HotelContent hotelDetails)
            => new ScheduleInfo(hotelDetails?.TimeInformation?.CheckTime.CheckIn, hotelDetails?.TimeInformation?.CheckTime.CheckOut);


    private List<MultilingualTextualDescription> GetMultiLingualDescription(JP_HotelContent hotelDetails)
    {
        var multilingualTextualDescription = new List<MultilingualTextualDescription>();

        if (hotelDetails.Descriptions is not null)
        {
            var multilingualDescription = new MultiLanguage<string>();

            foreach (var description in hotelDetails.Descriptions)
            {
                if (DescriptionTypes.Contains(description.Type))
                    multilingualDescription.TrySetValue(Constants.DefaultLanguageCode, description.Value);
            }

            multilingualTextualDescription.Add(new MultilingualTextualDescription(TextualDescriptionTypes.General, multilingualDescription));
        }        

        return multilingualTextualDescription;
    }


    private static UniqueAccommodationCodes GetUniqueCodes(JP_HotelContent hotelDetails)
        => new UniqueAccommodationCodes(hotelDetails.GiataCode);


    private List<string> DescriptionTypes = new() { "LNG", "ROO", "SPT", "OTH" };


    private readonly JuniperSerializer _serializer;    
}

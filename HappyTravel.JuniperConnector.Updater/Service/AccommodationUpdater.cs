using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Data.Models;
using HappyTravel.JuniperConnector.Updater.Infrastructure;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Updater.Settings;
using HappyTravel.JuniperConnector.Updater.Workers;
using JuniperServiceReference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System.Runtime.CompilerServices;

namespace HappyTravel.JuniperConnector.Updater.Service;

public class AccommodationUpdater : IUpdateWorker
{
    public AccommodationUpdater(JuniperContext context,
            ILogger<AccommodationUpdater> logger,
            JuniperSerializer serializer,
            IOptions<AccommodationDataUpdateOptions> options,
            DateTimeProvider dateTimeProvider)
    {
        _context = context;
        _logger = logger;
        _serializer = serializer;
        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }


    public async Task Run(CancellationToken cancellationToken)
    {
        _logger.LogStartingAccommodationUpdater();

        try
        {
            var zones = await _context.Zones.ToListAsync();
            await foreach (var hotels in GetHotelList(cancellationToken))
            {
                var existHotelCodes = hotels.Select(p => p.Code).ToList();
                var existingAccommodationCodes = await _context.Accommodations
                    .Where(a => existHotelCodes.Contains(a.Code))
                    .Select(a => a.Code)
                    .ToListAsync(cancellationToken);

                foreach (var hotel in hotels)
                {
                    var accommodation = Convert(hotel, zones);
                    if (accommodation is not default(Accommodation))
                    {
                        if (existingAccommodationCodes.Contains(hotel.Code))
                            _context.Accommodations.Update(accommodation);
                        else
                            _context.Accommodations.Add(accommodation);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                _context.ChangeTracker.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogAccommodationUpdaterException(ex);
        }
    }


    private Accommodation Convert(Hotel hotel, List<Zone> zones)
    {
        var data = _serializer.Deserialize<JP_HotelContent>(hotel.Data);

        var (country, locality) = GetCountryAndLocality(data.Zone.Code, zones);

        return new Accommodation
        {
            Code = hotel.Code,
            Country = country,
            Locality = locality,
            Name = data.HotelName,
            Coordinates = GetPoint(data?.Address?.Latitude, data?.Address?.Longitude),
            Modified = _dateTimeProvider.UtcNow()
        };


        static Point GetPoint(string latitudeString, string longitudeString)
            => double.TryParse(latitudeString, out double latitude) && double.TryParse(longitudeString, out double longitude)
            ? new Point(latitude, longitude)
            : new Point(0, 0);


        static (string, string) GetCountryAndLocality(string zoneCode, List<Zone> zones)
        {
            Zone zone;
            var zoneHierarchy = new List<Zone>();            

            do
            {
                zone = zones.Single(x => x.Code == zoneCode);
                zoneHierarchy.Add(zone);
                zoneCode = zone.ParentCode;                
            } while (zone.ParentCode != null);

            var countryZone = zoneHierarchy.FirstOrDefault(x => CountryAreaTypes.Contains(x.AreaType));
            var country = countryZone is not null ? countryZone.Name : string.Empty;

            var localityZone = zoneHierarchy.FirstOrDefault(x => LocalityAreaTypes.Contains(x.AreaType));
            var locality = localityZone is not null ? localityZone.Name : string.Empty;

            return (country, locality);
        }
    }


    private async IAsyncEnumerable<List<Hotel>> GetHotelList([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var lastUpdatedAccommodation = await GetDateSinceChanges();
        var i = 0;
        var propCount = await _context.Hotels.CountAsync(cancellationToken: cancellationToken);

        do
        {
            yield return await _context.Hotels
                .Where(r => r.Modified >= lastUpdatedAccommodation && r.IsActive)
                .OrderBy(r => r.Code)
                .Skip(i)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            _logger.LogAccommodationUpdaterGetHotels(i, i + BatchSize);
            i += BatchSize;

        } while (i < propCount);
    }


    private async Task<DateTimeOffset> GetDateSinceChanges()
    {
        if (_options.UpdateMode == UpdateMode.Full)
            return DateTimeOffset.MinValue;

        var lastModifiedAccommodation = await _context.Accommodations
            .OrderByDescending(a => a.Modified)
            .LastOrDefaultAsync();

        return lastModifiedAccommodation?.Modified ?? DateTimeOffset.MinValue;
    }


    private const int BatchSize = 1000;
    private static readonly IReadOnlyCollection<ZoneType> CountryAreaTypes = new ZoneType[] { ZoneType.PAS, ZoneType.COL };
    private static readonly IReadOnlyCollection<ZoneType> LocalityAreaTypes = new ZoneType[] { ZoneType.CTY, ZoneType.LOC, ZoneType.REG };


    private readonly JuniperContext _context;
    private readonly ILogger<AccommodationUpdater> _logger;
    private readonly JuniperSerializer _serializer;
    private readonly AccommodationDataUpdateOptions _options;
    private readonly DateTimeProvider _dateTimeProvider;
}

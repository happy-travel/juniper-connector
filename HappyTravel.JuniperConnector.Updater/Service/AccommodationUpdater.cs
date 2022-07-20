﻿using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Common.Infrastructure;
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
using System.Globalization;
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

        var (country, locality) = ZoneServiceExtensions.GetCountryAndLocality(data.Zone.Code, zones);

        var point = GetPoint(data?.Address?.Latitude, data?.Address?.Longitude);

        if (point is null)
        {
            _logger.LogPointParseFailed(data?.Address?.Latitude, data?.Address?.Longitude);
            point = new Point(0, 0);
        }


        return new Accommodation
        {
            Code = hotel.Code,
            Country = country,
            Locality = locality,
            Name = data.HotelName,
            Coordinates = point,
            Modified = _dateTimeProvider.UtcNow()
        };


        static Point? GetPoint(string latitudeString, string longitudeString)
        {
            return GetDouble(latitudeString, out double latitude) && GetDouble(longitudeString, out double longitude)
                ? new Point(latitude, longitude)
                : null;


            bool GetDouble(string str, out double result)
                => double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
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


    private readonly JuniperContext _context;
    private readonly ILogger<AccommodationUpdater> _logger;
    private readonly JuniperSerializer _serializer;
    private readonly AccommodationDataUpdateOptions _options;
    private readonly DateTimeProvider _dateTimeProvider;
}

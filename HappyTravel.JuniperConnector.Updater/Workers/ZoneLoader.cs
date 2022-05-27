using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Data.Models;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using JuniperServiceReference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.JuniperConnector.Updater.Workers;

public class ZoneLoader : IUpdateWorker
{
    public ZoneLoader(IJuniperServiceClient client, ILogger<ZoneLoader> logger, JuniperContext context)
    {
        _client = client;
        _logger = logger;
        _context = context;
    }


    public async Task Run(CancellationToken cancellationToken)
    {
        _logger.LogStartingZonesUpdate();

        try
        {
            var zoneCodes = await _context.Zones.Select(x => x.Code).ToListAsync();
            var zones = await _client.GetZoneList();

            await _context.Zones.AddRangeAsync(zones.Where(c => !zoneCodes.Contains(c.Code))
                                                    .Select(c => ConvertToZoneEntity(c)));

            _context.Zones.UpdateRange(zones.Where(c => zoneCodes.Contains(c.Code))
                                            .Select(c => ConvertToZoneEntity(c)));
        }
        catch (Exception ex)
        {
            _logger.LogZoneLoaderException(ex);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ChangeTracker.Clear();
    }


    private Zone ConvertToZoneEntity(JP_Zone zone)
        => new Zone
        {
            Code = zone.Code,
            Name = zone.Name,
            IATA = zone.IATA,
        };


    private readonly IJuniperServiceClient _client;
    private readonly ILogger<ZoneLoader> _logger;
    private readonly JuniperContext _context;    
}

using CSharpFunctionalExtensions;
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
    public ZoneLoader(JuniperContentClientService client, ILogger<ZoneLoader> logger, JuniperContext context)
    {
        _client = client;
        _logger = logger;
        _context = context;
    }


    public async Task Run(CancellationToken cancellationToken)
    {
        _logger.LogStartingZonesUpdate();
        
        var zoneCodes = await _context.Zones.Select(x => x.Code).ToListAsync();

        var (_, isFailure, zones, _) = await _client.GetZoneList();

        if (isFailure)
            return;
        
        await _context.Zones.AddRangeAsync(zones.Where(c => !zoneCodes.Contains(c.Code))
                                                .Select(c => ConvertToZoneEntity(c)));

        _context.Zones.UpdateRange(zones.Where(c => zoneCodes.Contains(c.Code))
                                        .Select(c => ConvertToZoneEntity(c)));

        await _context.SaveChangesAsync(cancellationToken);
        _context.ChangeTracker.Clear();

        _logger.LogFinishZonesUpdate();
    }


    private Zone ConvertToZoneEntity(JP_Zone zone)
        => new Zone
        {
            Code = zone.Code,
            Name = zone.Name,
            IATA = zone.IATA,
            ParentCode = zone.ParentCode,
            AreaType = Enum.TryParse(zone.AreaType, out ZoneType zoneType)
                                ? zoneType
                                : ZoneType.OTR
        };


    private readonly JuniperContentClientService _client;
    private readonly ILogger<ZoneLoader> _logger;
    private readonly JuniperContext _context;    
}

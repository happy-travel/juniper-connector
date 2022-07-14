using HappyTravel.BaseConnector.Api.Services.Locations;
using HappyTravel.EdoContracts.GeoData;
using HappyTravel.EdoContracts.GeoData.Enums;
using HappyTravel.JuniperConnector.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.JuniperConnector.Api.Services.Locations;

public class LocationService : ILocationService
{
    public LocationService(JuniperContext context, LocationMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<List<Location>> Get(DateTimeOffset modified, LocationTypes locationType, int skip, int top, CancellationToken cancellationToken)
    {
        return locationType switch
        {
            LocationTypes.Destination => new List<Location>(),
            LocationTypes.Accommodation => await GetAccommodations(),
            LocationTypes.Landmark => new List<Location>(),
            LocationTypes.Location => await GetLocalities(),
            _ => throw new ArgumentException($"Invalid location type {locationType}")
        };


        async Task<List<Location>> GetAccommodations()
        {
            var accommodations = await _context.Accommodations
                .Where(a => a.Modified >= modified)
                .OrderBy(a => a.Code)
                .Skip(skip)
                .Take(top)
                .ToListAsync(cancellationToken);

            return accommodations
                .Select(_mapper.MapAccommodationToLocation)
                .ToList();
        }


        async Task<List<Location>> GetLocalities()
        {
            var localityNames = await _context.Accommodations
                .Where(a => a.Modified >= modified)
                .OrderBy(a => a.Code)
                .Select(a => new { a.Country, a.Locality })
                .Distinct()
                .Skip(skip)
                .Take(top)
                .ToListAsync(cancellationToken);

            return localityNames
                .Select(n => _mapper.MapLocalityToLocation(n.Country, n.Locality))
                .ToList();
        }
    }


    private readonly JuniperContext _context;
    private readonly LocationMapper _mapper;
}
